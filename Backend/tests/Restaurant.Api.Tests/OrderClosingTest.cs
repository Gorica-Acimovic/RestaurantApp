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
    public class OrderClosingTest : IClassFixture<ApiFactory>
    {
        private readonly ApiFactory _factory;

        public OrderClosingTest(ApiFactory factory) 
        {  
            _factory = factory; 
        }

        [Fact]
        public async Task Closing_an_order_issues_the_invoice_and_frees_the_table()
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
                quantity = 2
            });
            addItem.EnsureSuccessStatusCode();

            
            var close = await admin.PostAsJsonAsync($"/api/Orders/{orderId}/close", new
            {
                paymentMethod = "Cash"
            });
            
            Assert.Equal(HttpStatusCode.OK, close.StatusCode);

            var invoice = await close.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(21.00m, invoice.GetProperty("totalAmount").GetDecimal());
            Assert.StartsWith("INV-", invoice.GetProperty("invoiceNumberText").GetString());
           
            var tables = await admin.GetFromJsonAsync<JsonElement>("/api/RestaurantTables");
            var table = tables.EnumerateArray().First(t => t.GetProperty("id").GetInt32() == tableId);
            Assert.Equal("Available", table.GetProperty("status").GetString());
           
            var openOrder = await admin.GetAsync($"/api/Orders/table/{tableId}");
            Assert.Equal(HttpStatusCode.NotFound, openOrder.StatusCode);
            
            var savedInvoice = await admin.GetAsync($"/api/Orders/{orderId}/invoice");
            Assert.Equal(HttpStatusCode.OK, savedInvoice.StatusCode);
        }



        [Fact]
        public async Task Closing_an_empty_order_is_rejected_and_the_table_stays_occupied()
        {
            var admin = _factory.CreateClient();
            await TestHelper.LoginAsAdminAsync(admin);

            var tableId = await TestHelper.CreateTableAsync(admin);
            var orderId = await TestHelper.OpenOrderAsync(admin, tableId);
           
            var close = await admin.PostAsJsonAsync($"/api/Orders/{orderId}/close", new
            {
                paymentMethod = "Cash"
            });
           
            Assert.Equal(HttpStatusCode.BadRequest, close.StatusCode);

            var tables = await admin.GetFromJsonAsync<JsonElement>("/api/RestaurantTables");
            var table = tables.EnumerateArray().First(t => t.GetProperty("id").GetInt32() == tableId);
            Assert.Equal("Occupied", table.GetProperty("status").GetString());

            var openOrder = await admin.GetAsync($"/api/Orders/table/{tableId}");
            Assert.Equal(HttpStatusCode.OK, openOrder.StatusCode);
        }
    }

}

