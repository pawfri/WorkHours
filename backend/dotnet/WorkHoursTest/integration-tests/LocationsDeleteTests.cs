using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user, I want to delete a workplace I created by mistake,
// so that my list only contains real workplaces.
public class LocationsDeleteTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public LocationsDeleteTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<Location> CreateLocation(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Location>())!;
    }

    // Shifts may not overlap, so tests that create several shifts give each its own day.
    private async Task<Shift> CreateShift(int locationId, int dayOffset = 0)
    {
        var day = new DateTime(2026, 1, 5).AddDays(dayOffset);
        var response = await _client.PostAsJsonAsync("/api/shifts", new
        {
            startTime = day.AddHours(8),
            endTime = day.AddHours(16),
            userId = 1,
            locationId
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Shift>())!;
    }

    [Fact]
    public async Task Delete_LocationWithoutShifts_Returns204()
    {
        // Arrange
        var location = await CreateLocation("Office");

        // Act
        var response = await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_LocationWithoutShifts_RemovedFromListAndGetById()
    {
        // Arrange
        var location = await CreateLocation("Office");

        // Act
        await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        var all = await _client.GetFromJsonAsync<List<Location>>("/api/locations");
        Assert.Empty(all!);
        var get = await _client.GetAsync($"/api/locations/{location.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Delete_DoesNotAffectOtherLocations()
    {
        // Arrange
        var keep = await CreateLocation("Warehouse");
        var remove = await CreateLocation("Office");

        // Act
        await _client.DeleteAsync($"/api/locations/{remove.Id}");

        // Assert
        var all = await _client.GetFromJsonAsync<List<Location>>("/api/locations");
        var only = Assert.Single(all!);
        Assert.Equal(keep.Id, only.Id);
    }

    [Fact]
    public async Task Delete_LocationUsedOnlyByOtherLocationsShifts_Returns204()
    {
        // Arrange
        var other = await CreateLocation("Warehouse");
        var office = await CreateLocation("Office");
        await CreateShift(other.Id);

        // Act
        var response = await _client.DeleteAsync($"/api/locations/{office.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_LocationWithOneShift_Returns409MentioningCount()
    {
        // Arrange
        var location = await CreateLocation("Office");
        await CreateShift(location.Id);

        // Act
        var response = await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("1 shift", body);
    }

    [Fact]
    public async Task Delete_LocationWithMultipleShifts_Returns409MentioningCount()
    {
        // Arrange
        var location = await CreateLocation("Office");
        await CreateShift(location.Id, 0);
        await CreateShift(location.Id, 1);
        await CreateShift(location.Id, 2);

        // Act
        var response = await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("3 shift", body);
    }

    [Fact]
    public async Task Delete_RefusedBecauseOfShifts_LeavesLocationAndShiftsUnchanged()
    {
        // Arrange
        var location = await CreateLocation("Office");
        var shift1 = await CreateShift(location.Id, 0);
        var shift2 = await CreateShift(location.Id, 1);

        // Act
        await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        var locations = await _client.GetFromJsonAsync<List<Location>>("/api/locations");
        var remaining = Assert.Single(locations!);
        Assert.Equal(location.Id, remaining.Id);
        Assert.Equal("Office", remaining.Name);
        var shifts = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Equal(2, shifts!.Count);
        Assert.Contains(shifts, s => s.Id == shift1.Id && s.LocationId == location.Id);
        Assert.Contains(shifts, s => s.Id == shift2.Id && s.LocationId == location.Id);
    }

    [Fact]
    public async Task Delete_AfterShiftsRemoved_Returns204()
    {
        // Arrange
        var location = await CreateLocation("Office");
        var shift = await CreateShift(location.Id);
        await _client.DeleteAsync($"/api/shifts/{shift.Id}");

        // Act
        var response = await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_Returns404()
    {
        // Act
        var response = await _client.DeleteAsync("/api/locations/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SameLocationTwice_SecondReturns404()
    {
        // Arrange
        var location = await CreateLocation("Office");
        await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Act
        var response = await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
