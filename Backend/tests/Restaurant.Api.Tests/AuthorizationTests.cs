using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class AuthorizationTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public AuthorizationTests(ApiFactory factory) 
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("anonymous", "POST", "/api/RestaurantTables", 401)]
        [InlineData("guest", "POST", "/api/RestaurantTables", 403)]
        [InlineData("anonymous", "GET", "/api/Reservations", 401)]
        [InlineData("guest", "GET", "/api/Reservations", 403)]
        [InlineData("guest", "GET", "/api/Kitchen/tickets", 403)]
        [InlineData("guest", "GET", "/api/RestaurantTables", 403)]
        [InlineData("admin", "GET", "/api/Reservations", 200)]

        public async Task Protected_endpoints_return_the_expected_status_for_each_role (string role, string method, string url, int expextedStatus)
        {
            var client = _factory.CreateClient();

            if(role == "admin")
            {
                await TestHelper.LoginAsAdminAsync(client);
            }
            else if (role == "guest")
            {
                await TestHelper.LoginAsNewGuestAsync(client);

            }


            var request = new HttpRequestMessage(new HttpMethod(method), url);

            if(method == "POST")
            {
                request.Content = JsonContent.Create(new { });        
            }

            var response = await client.SendAsync(request);          

            Assert.Equal((HttpStatusCode)expextedStatus, response.StatusCode);
        }

    }
}
