using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user, I want to change a workplace's name or address,
// so that my information stays correct.
public class LocationsUpdateTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public LocationsUpdateTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<Location> CreateLocation(string name, string? address = null, string? city = null, string? zipCode = null)
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name, address, city, zipCode });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Location>())!;
    }

    [Fact]
    public async Task Update_Name_Returns204AndPersists()
    {
        // Arrange
        var location = await CreateLocation("Office", "Main Street 1", "Copenhagen", "1000");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}",
            new { name = "HQ", address = "Main Street 1", city = "Copenhagen", zipCode = "1000" });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{location.Id}");
        Assert.Equal("HQ", fetched!.Name);
    }

    [Fact]
    public async Task Update_Address_Returns204AndPersists()
    {
        // Arrange
        var location = await CreateLocation("Office", "Main Street 1", "Copenhagen", "1000");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}",
            new { name = "Office", address = "Side Street 2", city = "Aarhus", zipCode = "8000" });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{location.Id}");
        Assert.Equal("Office", fetched!.Name);
        Assert.Equal("Side Street 2", fetched.Address);
        Assert.Equal("Aarhus", fetched.City);
        Assert.Equal("8000", fetched.ZipCode);
    }

    [Fact]
    public async Task Update_KeepsId()
    {
        // Arrange
        var location = await CreateLocation("Office");

        // Act
        await _client.PutAsJsonAsync($"/api/locations/{location.Id}", new { name = "HQ" });

        // Assert
        var all = await _client.GetFromJsonAsync<List<Location>>("/api/locations");
        var only = Assert.Single(all!);
        Assert.Equal(location.Id, only.Id);
    }

    [Fact]
    public async Task Update_OmittedOptionalFields_AreCleared()
    {
        // Arrange
        var location = await CreateLocation("Office", "Main Street 1", "Copenhagen", "1000");

        // Act
        await _client.PutAsJsonAsync($"/api/locations/{location.Id}", new { name = "Office" });

        // Assert
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{location.Id}");
        Assert.Null(fetched!.Address);
        Assert.Null(fetched.City);
        Assert.Null(fetched.ZipCode);
    }

    [Fact]
    public async Task Update_TrimsName()
    {
        // Arrange
        var location = await CreateLocation("Office");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}", new { name = "  HQ  " });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{location.Id}");
        Assert.Equal("HQ", fetched!.Name);
    }

    [Fact]
    public async Task Update_KeepingOwnName_Returns204()
    {
        // Arrange
        var location = await CreateLocation("Office", "Main Street 1");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}",
            new { name = "office", address = "Side Street 2" });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{location.Id}");
        Assert.Equal("office", fetched!.Name);
        Assert.Equal("Side Street 2", fetched.Address);
    }

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        // Act
        var response = await _client.PutAsJsonAsync("/api/locations/999", new { name = "HQ" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Location>>("/api/locations");
        Assert.Empty(all!);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":null}")]
    [InlineData("{\"name\":\"\"}")]
    [InlineData("{\"name\":\"   \"}")]
    public async Task Update_MissingOrBlankName_Returns400AndLeavesLocationUnchanged(string json)
    {
        // Arrange
        var location = await CreateLocation("Office", "Main Street 1");

        // Act
        var response = await _client.PutAsync($"/api/locations/{location.Id}",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{location.Id}");
        Assert.Equal("Office", fetched!.Name);
        Assert.Equal("Main Street 1", fetched.Address);
    }

    [Fact]
    public async Task Update_NameOf100Chars_Returns204()
    {
        // Arrange
        var location = await CreateLocation("Office");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}", new { name = new string('a', 100) });

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_NameOf101Chars_Returns400()
    {
        // Arrange
        var location = await CreateLocation("Office");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{location.Id}", new { name = new string('a', 101) });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("warehouse")]
    [InlineData("WAREHOUSE")]
    [InlineData("  warehouse ")]
    public async Task Update_NameUsedByAnotherLocation_Returns409AndLeavesLocationUnchanged(string duplicate)
    {
        // Arrange
        await CreateLocation("Warehouse");
        var office = await CreateLocation("Office");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/locations/{office.Id}", new { name = duplicate });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{office.Id}");
        Assert.Equal("Office", fetched!.Name);
    }

    [Fact]
    public async Task Update_DoesNotAffectOtherLocations()
    {
        // Arrange
        var other = await CreateLocation("Warehouse", "Dock Road 5");
        var office = await CreateLocation("Office");

        // Act
        await _client.PutAsJsonAsync($"/api/locations/{office.Id}", new { name = "HQ", address = "Main Street 1" });

        // Assert
        var fetched = await _client.GetFromJsonAsync<Location>($"/api/locations/{other.Id}");
        Assert.Equal("Warehouse", fetched!.Name);
        Assert.Equal("Dock Road 5", fetched.Address);
    }
}
