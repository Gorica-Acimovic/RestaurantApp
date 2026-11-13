using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Api.Tests
{
    public class OrderConcurrencyTest : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;
        public OrderConcurrencyTest(ApiFactory factory ) 
        { 
            _factory = factory;
        }

        [Fact]
        public async Task Ten_parallel_closes_of_the_same_order_issue_only_one_invoice()
        {
         
            var admin = _factory.CreateClient();
            await TestHelper.LoginAsAdminAsync(admin);

            var manager = _factory.CreateClient();
            await TestHelper.LoginAsNewManagerAsync(admin, manager);

            var menuItemId = await TestHelper.CreateMenuItemAsync(manager, 10.50m);
            var tableId = await TestHelper.CreateTableAsync(admin);
            var orderId = await TestHelper.OpenOrderAsync(admin, tableId);

            var addItem = await admin.PostAsJsonAsync($"/api/Orders/{orderId}/items", new
            {
                menuItemId = menuItemId,
                quantity = 1
            });
            addItem.EnsureSuccessStatusCode();
            
            var tasks = Enumerable.Range(1, 10).Select(_ =>
                admin.PostAsJsonAsync($"/api/Orders/{orderId}/close", new { paymentMethod = "Cash" }));

            var responses = await Task.WhenAll(tasks);

            var ok = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflict = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

            Assert.Equal(1, ok);
            Assert.Equal(9, conflict);

            var invoice = await admin.GetAsync($"/api/Orders/{orderId}/invoice");
            Assert.Equal(HttpStatusCode.OK, invoice.StatusCode);
        }

        [Fact]
        public async Task Ten_parallel_opens_on_the_same_table_create_only_one_order()
        {
            var admin = _factory.CreateClient();
            await TestHelper.LoginAsAdminAsync(admin);

            var tableId = await TestHelper.CreateTableAsync(admin);
           
            var tasks = Enumerable.Range(1, 10).Select(_ =>
                admin.PostAsJsonAsync("/api/Orders", new { tableId = tableId, numberOfGuests = 2 }));

            var responses = await Task.WhenAll(tasks);

            var ok = responses.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflict = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

            Assert.Equal(1, ok);
            Assert.Equal(9, conflict);

            var openOrder = await admin.GetAsync($"/api/Orders/table/{tableId}");
            Assert.Equal(HttpStatusCode.OK, openOrder.StatusCode);
        }
    }
}
