using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;

namespace WorkHoursTest.IntegrationTests;

// User story: As an employee, I want to save the places where I work,
// so that I can record where each shift took place.
public class LocationsCreateTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public LocationsCreateTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Create_WithAllFields_Returns201WithIdAndLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/locations",
            new { name = "Office", address = "Main Street 1", city = "Copenhagen", zipCode = "1000" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Location>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Office", created.Name);
        Assert.Equal("Main Street 1", created.Address);
        Assert.Equal("Copenhagen", created.City);
        Assert.Equal("1000", created.ZipCode);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/Locations/{created.Id}", response.Headers.Location.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithNameOnly_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = "Warehouse" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Location>();
        Assert.NotNull(created);
        Assert.Null(created.Address);
        Assert.Null(created.City);
        Assert.Null(created.ZipCode);
    }

    [Fact]
    public async Task Create_LinkReturned_ResolvesToCreatedLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = "Office" });

        var fetched = await _client.GetFromJsonAsync<Location>(response.Headers.Location);
        Assert.NotNull(fetched);
        Assert.Equal("Office", fetched.Name);
    }

    [Fact]
    public async Task Create_TrimsName()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = "  Office  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<Location>();
        Assert.Equal("Office", created!.Name);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":null}")]
    [InlineData("{\"name\":\"\"}")]
    [InlineData("{\"name\":\"   \"}")]
    public async Task Create_MissingOrBlankName_Returns400(string json)
    {
        var response = await _client.PostAsync("/api/locations",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NameOf100Chars_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = new string('a', 100) });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_NameOf101Chars_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = new string('a', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_NameOf100CharsWithSurroundingSpaces_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = "  " + new string('a', 100) + "  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("office")]
    [InlineData("OFFICE")]
    [InlineData("  office ")]
    public async Task Create_DuplicateNameIgnoringCase_Returns409(string duplicate)
    {
        await _client.PostAsJsonAsync("/api/locations", new { name = "Office" });

        var response = await _client.PostAsJsonAsync("/api/locations", new { name = duplicate });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Location>>("/api/locations");
        Assert.Single(all!);
    }

    [Fact]
    public async Task Create_AppearsInListImmediately()
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name = "Office" });
        var created = await response.Content.ReadFromJsonAsync<Location>();

        var all = await _client.GetFromJsonAsync<List<Location>>("/api/locations");

        Assert.Contains(all!, l => l.Id == created!.Id && l.Name == "Office");
    }
}
