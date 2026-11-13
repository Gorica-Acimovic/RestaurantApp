namespace Restaurant.Api.Tests;

public class SmokeTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Menu_list_is_public_and_returns_200()
    {
        var response = await _client.GetAsync("/api/MenuItems");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}