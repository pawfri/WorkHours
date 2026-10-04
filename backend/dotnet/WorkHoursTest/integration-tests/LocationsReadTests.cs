using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user, I want to see the workplaces I've saved,
// so that I can choose the right one when I record a shift.
public class LocationsReadTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public LocationsReadTests()
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

    private async Task<List<Location>> GetAll()
    {
        return (await _client.GetFromJsonAsync<List<Location>>("/api/locations"))!;
    }

    [Fact]
    public async Task List_WithNoLocations_Returns200WithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/locations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var all = await response.Content.ReadFromJsonAsync<List<Location>>();
        Assert.NotNull(all);
        Assert.Empty(all);
    }

    [Fact]
    public async Task List_ReturnsLocationsSortedAlphabeticallyByName()
    {
        // Arrange
        await CreateLocation("Warehouse");
        await CreateLocation("Office");
        await CreateLocation("Bakery");

        // Act
        var all = await GetAll();

        // Assert
        Assert.Equal(["Bakery", "Office", "Warehouse"], all.Select(l => l.Name));
    }

    [Fact]
    public async Task List_SortingIgnoresCase()
    {
        // Arrange
        await CreateLocation("bakery");
        await CreateLocation("Office");
        await CreateLocation("Archive");

        // Act
        var all = await GetAll();

        // Assert
        Assert.Equal(["Archive", "bakery", "Office"], all.Select(l => l.Name));
    }

    [Fact]
    public async Task List_EntryShowsNameAddressCityAndZipCode()
    {
        // Arrange
        var created = await CreateLocation("Office", "Main Street 1", "Copenhagen", "1000");

        // Act
        var all = await GetAll();

        // Assert
        var entry = Assert.Single(all);
        Assert.Equal(created.Id, entry.Id);
        Assert.Equal("Office", entry.Name);
        Assert.Equal("Main Street 1", entry.Address);
        Assert.Equal("Copenhagen", entry.City);
        Assert.Equal("1000", entry.ZipCode);
    }

    [Fact]
    public async Task GetById_ReturnsTheRequestedLocation()
    {
        // Arrange
        await CreateLocation("Bakery", "Baker Street 2", "Aarhus", "8000");
        var office = await CreateLocation("Office", "Main Street 1", "Copenhagen", "1000");

        // Act
        var response = await _client.GetAsync($"/api/locations/{office.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<Location>();
        Assert.NotNull(fetched);
        Assert.Equal(office.Id, fetched.Id);
        Assert.Equal("Office", fetched.Name);
        Assert.Equal("Main Street 1", fetched.Address);
        Assert.Equal("Copenhagen", fetched.City);
        Assert.Equal("1000", fetched.ZipCode);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/locations/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
