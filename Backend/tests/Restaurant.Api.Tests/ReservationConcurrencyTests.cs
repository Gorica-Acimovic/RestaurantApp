using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class ReservationConcurrencyTests : IClassFixture<ApiFactory> 
    {
        private readonly ApiFactory _factory;  
        private readonly HttpClient _httpClient; 

        public ReservationConcurrencyTests(ApiFactory factory)
        {
            _factory = factory;
            _httpClient = factory.CreateClient();
        }

        [Fact]    

        public async Task Ten_parallel_reservations_for_the_same_table_and_time_only_one_succeeds()   
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
            tableResponse.EnsureSuccessStatusCode();

            var table = await tableResponse.Content.ReadFromJsonAsync<JsonElement>();
            var tableId = table.GetProperty("id").GetInt32();

            var startsAt = DateTime.Today.AddDays(1).AddHours(18);
           
            var tasks = Enumerable.Range(1, 10).Select(i =>             
            {
                var guestClient = _factory.CreateClient();             
                return guestClient.PostAsJsonAsync("/api/Reservations", new            
                {
                    customerName = $"Guest {i}",
                    startsAt = startsAt,
                    numberOfGuests = 2,
                    tableId = tableId
                });
            });

            var responses = await Task.WhenAll(tasks);              

            var ok = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.OK);     
            var conflict = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Conflict); 

            Assert.Equal(1, ok);            
            Assert.Equal(9, conflict);

        }

    }
}
