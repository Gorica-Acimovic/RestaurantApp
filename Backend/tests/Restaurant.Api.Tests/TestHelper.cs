using Restaurant.Api.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Serilog;

namespace Restaurant.Api.Tests
{
    public class TestHelper
    {
        private static int _tableNumber = 100;

        public static async Task LoginAsAdminAsync (HttpClient client)
        {
            var response = await client.PostAsJsonAsync("/api/Auth/login", new
            {
                email = "admin@test.local",
                password = "Admin#12345"

            });
            response.EnsureSuccessStatusCode();

            var login = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = login.GetProperty("accessToken").GetString();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task LoginAsNewGuestAsync (HttpClient client)
        {
            var response = await client.PostAsJsonAsync("/api/Auth/register", new
            {
                firstName = "Test",
                lastName = "Guest",
                email = $"guest{Guid.NewGuid():N}@test.local",     
                password = "Guest#12345"

            });
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = body.GetProperty("accessToken").GetString();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


        public static async Task<int> CreateTableAsync (HttpClient adminClient, int capacity = 4)
        {
            var number = Interlocked.Increment(ref _tableNumber);

            var response = await adminClient.PostAsJsonAsync("/api/RestaurantTables", new
            {
                number = number,
                capacity = capacity,
                positionX = 10,
                positionY = 10


            });
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("id").GetInt32();

        }


        public static Task<HttpResponseMessage> CreateReservationAsync (HttpClient client, int tableId, DateTime startsAt, int guests = 2)
        {
            return client.PostAsJsonAsync("/api/Reservations", new
            {
                customerName = "Test Guest",
                startsAt = startsAt,
                numberOfGuests = guests,
                tableId = tableId
            });
        }


        public static async Task LoginAsNewManagerAsync (HttpClient adminClient, HttpClient managerClient)
        {
            var email = $"manager{Guid.NewGuid():N}@test.local";
            const string password = "Manager#12345";

            var register = await adminClient.PostAsJsonAsync("/api/Managers", new
            {
                firstName = "Test",
                lastName = "Manager",
                email = email,
                password = password
            });
            register.EnsureSuccessStatusCode();

            var login = await managerClient.PostAsJsonAsync("/api/Auth/login", new
            {
                email = email,
                password = password
            });

            login.EnsureSuccessStatusCode();

            var body = await login.Content.ReadFromJsonAsync<JsonElement>();

            var token = body.GetProperty("accessToken").GetString();

            managerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public static async Task<int> CreateMenuItemAsync(HttpClient managerClient, decimal price)
        {
            var response = await managerClient.PostAsJsonAsync("/api/MenuItems", new
            {
                name = $"Test dish {Guid.NewGuid():N}",
                description = "Test",
                price = price,
                subcategoryId = 3
            });
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("id").GetInt32();
        }

        public static async Task<int> OpenOrderAsync(HttpClient client, int tableId, int guests = 2)
        {
            var response = await client.PostAsJsonAsync("/api/Orders", new
            {
                tableId = tableId,
                numberOfGuests = guests
            });
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("id").GetInt32();
        }
    }
}
