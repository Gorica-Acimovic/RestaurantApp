using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class ReservationRulesTest : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public ReservationRulesTest(ApiFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(-1, 18, 2, 400)]   
        [InlineData(1, 9, 2, 400)]     
        [InlineData(1, 22, 2, 400)]  
        [InlineData(31, 18, 2, 400)]  
        [InlineData(1, 18, 5, 400)]   
        [InlineData(1, 18, 2, 200)] 
        public async Task Reservation_rules_are_enforced(int dayOffset, int hour, int guests, int expectedStatus)
        {
          
            var admin = _factory.CreateClient();
            await TestHelper.LoginAsAdminAsync(admin);
            var tableId = await TestHelper.CreateTableAsync(admin, capacity: 4);

            var startsAt = DateTime.Today.AddDays(dayOffset).AddHours(hour);
           
            var response = await TestHelper.CreateReservationAsync(_factory.CreateClient(), tableId, startsAt, guests);

            Assert.Equal((HttpStatusCode)expectedStatus, response.StatusCode);
        }
    }
}
