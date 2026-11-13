using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class ReservationRowVersionTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        private readonly HttpClient _httpClient;
        public ReservationRowVersionTests( ApiFactory factory) 
        {
            _factory = factory;
            _httpClient = factory.CreateClient();
        
        }

        [Fact]

        public async Task Changing_status_with_a_stale_row_version_returns_409()
        {
            var loginResponse = await _httpClient.PostAsJsonAsync("/api/Auth/login", new
            {
                email = "admin@test.local",
                password = "Admin#12345"
            });
            loginResponse.EnsureSuccessStatusCode();

            var login = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            var token = login.GetProperty("accessToken").GetString();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var tableResponse = await _httpClient.PostAsJsonAsync("/api/RestaurantTables", new
            {
                number = 1,
                capacity = 4,
                positionX = 10,
                positionY = 10

            });

            var table = await tableResponse.Content.ReadFromJsonAsync<JsonElement>();
            var tableId = table.GetProperty("id").GetInt32();

            var guestClient = _factory.CreateClient();
            var createResponse = await guestClient.PostAsJsonAsync("/api/Reservations", new
            {
                customerName = "Guest One",
                startsAt = DateTime.Today.AddDays(1).AddHours(18),
                numberOfGuests = 2,
                tableId = tableId

            });
            createResponse.EnsureSuccessStatusCode();

            var reservation = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
            var reservationId = reservation.GetProperty("id").GetInt32();
            var oldRowVersion = reservation.GetProperty("rowVersion").GetString();          

            var confirmResponse = await _httpClient.PutAsJsonAsync($"/api/Reservations/{reservationId}/status", new
            {
                status = "Confirmed",
                rowVersion = oldRowVersion

            });

            var cancelResponse = await _httpClient.PutAsJsonAsync($"/api/Reservations/{reservationId}/status", new
            {
                status = "Cancelled",
                rowVersion = oldRowVersion

            });


            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
        }
    }
}
