using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class ReservationOverlapTests : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public ReservationOverlapTests(ApiFactory factory)
        {
            _factory = factory;
        }

        [Theory]                     
        [InlineData(-120,200)]
        [InlineData(-60,409)]
        [InlineData(0,409)]
        [InlineData(60,409)]
        [InlineData(120,200)]


        public async Task Second_reservation_on_the_same_table_respects_the_overlap_rule (int minutesFromBase, int expectedStatus)
        {
            var admin = _factory.CreateClient();
            await TestHelper.LoginAsAdminAsync(admin);
            var tableId = await TestHelper.CreateTableAsync(admin);

            var baseStart = DateTime.Today.AddDays(1).AddHours(18);

            var first = await TestHelper.CreateReservationAsync(_factory.CreateClient(), tableId, baseStart);
            first.EnsureSuccessStatusCode();      

            var second = await TestHelper.CreateReservationAsync(_factory.CreateClient(), tableId, baseStart.AddMinutes(minutesFromBase));

            Assert.Equal((HttpStatusCode)expectedStatus, second.StatusCode);    

        }                   
    }
}
