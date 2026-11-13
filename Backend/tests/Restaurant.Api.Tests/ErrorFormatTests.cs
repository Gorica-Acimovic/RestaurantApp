using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class ErrorFormatTests :IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public ErrorFormatTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("anonymous", "/api/Reservations", 401)]
        [InlineData("guest", "/api/Reservations", 403)]
        [InlineData("admin", "/api/Orders/table/999999", 404)]
        [InlineData("anonymous", "/api/NoSuchRoute", 404)]
        public async Task Errors_use_the_unified_format(string role, string url, int expectedStatus)
        {
         
            var client = _factory.CreateClient();

            if (role == "admin")
            {
                await TestHelper.LoginAsAdminAsync(client);
            }
            else if (role == "guest")
            {
                await TestHelper.LoginAsNewGuestAsync(client);
            }

            
            var response = await client.GetAsync(url);

           
            Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(expectedStatus, error.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("message").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("traceId").GetString()));
        }

        [Fact]
        public async Task Validation_errors_contain_a_message_and_an_errors_object()
        {
           
            var client = _factory.CreateClient();
            
            var response = await client.PostAsJsonAsync("/api/Reservations", new
            {
                customerName = "",
                startsAt = DateTime.Today.AddDays(1).AddHours(18),
                numberOfGuests = 0
            });
                       
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(400, error.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("message").GetString()));
            Assert.True(error.GetProperty("errors").EnumerateObject().Any());
        }
    }
}
