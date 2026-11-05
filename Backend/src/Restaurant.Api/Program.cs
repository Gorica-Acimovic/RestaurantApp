using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Restaurant.Api.Services;
using Restaurant.Api.Interfaces;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Restaurant.Api.Models;
using Restaurant.Api.Hubs;
using Restaurant.Api.Workers;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Infrastructure;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());


builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

            var message = errors.Values.First().First();

            return new BadRequestObjectResult(ApiError.Create(context.HttpContext, StatusCodes.Status400BadRequest, message, errors));
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPinHasher, PinHasherService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOnlineOrderService, OnlineOrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IForecastService, ForecastService>();
builder.Services.AddScoped<IKitchenService, KitchenService>();

builder.Services.AddHostedService<DailyStatisticsWorker>();
builder.Services.AddHostedService<ReservationLifecycleWorker>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("Reservations"));
builder.Services.Configure<OnlineOrderOptions>(builder.Configuration.GetSection("OnlineOrders"));
builder.Services.AddSingleton(TimeProvider.System);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/restaurant"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }

        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("pin-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT token here."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
});



builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader()
                .AllowCredentials();
    });
});
var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseStatusCodePages(async context =>
{
    var httpContext = context.HttpContext;
    var status = httpContext.Response.StatusCode;

    var message = status switch
    {
        StatusCodes.Status401Unauthorized => "You must be signed in to do this.",
        StatusCodes.Status403Forbidden => "You are not allowed to do this.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status429TooManyRequests => "Too many requests. Please try again later.",
        _ => "The request could not be completed."
    };

    await httpContext.Response.WriteAsJsonAsync(ApiError.Create(httpContext, status, message));
});


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(dbContext, app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RestaurantHub>("/hubs/restaurant");

app.Run();
