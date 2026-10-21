using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;
using WorkHoursRest.Dtos;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user, I want overlapping shifts refused, so that the same time is never counted twice.
//
// An overlap is refused with 409 Conflict, whatever the workplace. Shifts that only touch do not overlap.
public class ShiftsOverlapTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ShiftsOverlapTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<Location> CreateLocation(string name = "Office")
    {
        var response = await _client.PostAsJsonAsync("/api/locations", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Location>())!;
    }

    private static StringContent ShiftJson(int locationId, string startTime, string endTime)
    {
        var json = $"{{\"startTime\":\"{startTime}\",\"endTime\":\"{endTime}\",\"locationId\":{locationId}}}";
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private Task<HttpResponseMessage> PostShift(int locationId, string startTime, string endTime)
    {
        return _client.PostAsync("/api/shifts", ShiftJson(locationId, startTime, endTime));
    }

    private Task<HttpResponseMessage> PutShift(int id, int locationId, string startTime, string endTime)
    {
        return _client.PutAsync($"/api/shifts/{id}", ShiftJson(locationId, startTime, endTime));
    }

    private async Task<ShiftResponseDto> CreateShift(int locationId, string startTime, string endTime)
    {
        var response = await PostShift(locationId, startTime, endTime);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ShiftResponseDto>())!;
    }

    // Overlapping shifts are refused

    [Theory]
    [InlineData("2026-01-05T11:00:00", "2026-01-05T13:00:00")] // overlaps the end
    [InlineData("2026-01-05T07:00:00", "2026-01-05T09:00:00")] // overlaps the start
    [InlineData("2026-01-05T09:00:00", "2026-01-05T10:00:00")] // inside
    [InlineData("2026-01-05T07:00:00", "2026-01-05T13:00:00")] // covers
    [InlineData("2026-01-05T08:00:00", "2026-01-05T12:00:00")] // identical
    public async Task Create_OverlappingShift_Returns409AndDoesNotSaveShift(string start, string end)
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PostShift(location.Id, start, end);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Single(all!);
    }

    [Fact]
    public async Task Create_OverlappingShiftAtDifferentLocation_Returns409()
    {
        // Arrange
        var office = await CreateLocation("Office");
        var cafe = await CreateLocation("Cafe");
        await CreateShift(office.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PostShift(cafe.Id, "2026-01-05T11:00:00", "2026-01-05T13:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShiftOverlappingAcrossMidnight_Returns409()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Act
        var response = await PostShift(location.Id, "2026-01-06T05:00:00", "2026-01-06T09:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // The message names the conflicting shift and its times

    [Fact]
    public async Task Create_OverlappingShift_MessageNamesConflictingShiftAndTimes()
    {
        // Arrange
        var location = await CreateLocation();
        var existing = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PostShift(location.Id, "2026-01-05T11:00:00", "2026-01-05T13:00:00");

        // Assert
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal($"Shift overlaps with shift {existing.Id} from 2026-01-05 08:00 to 2026-01-05 12:00.", problem!.Detail);
    }

    [Fact]
    public async Task Create_OverlappingShiftAcrossMidnight_MessageIncludesBothDates()
    {
        // Arrange
        var location = await CreateLocation();
        var existing = await CreateShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Act
        var response = await PostShift(location.Id, "2026-01-06T05:00:00", "2026-01-06T09:00:00");

        // Assert
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal($"Shift overlaps with shift {existing.Id} from 2026-01-05 22:00 to 2026-01-06 06:00.", problem!.Detail);
    }

    // Shifts that only touch are not overlaps

    [Theory]
    [InlineData("2026-01-05T12:00:00", "2026-01-05T16:00:00")] // starts when the other ends
    [InlineData("2026-01-05T04:00:00", "2026-01-05T08:00:00")] // ends when the other starts
    public async Task Create_ShiftTouchingAnother_Returns201(string start, string end)
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PostShift(location.Id, start, end);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShiftTouchingAnotherAcrossMidnight_Returns201()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Act
        var response = await PostShift(location.Id, "2026-01-06T06:00:00", "2026-01-06T09:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // Updating a shift follows the same rules

    [Fact]
    public async Task Update_OntoAnotherShift_Returns409AndDoesNotChangeShift()
    {
        // Arrange
        var location = await CreateLocation();
        var morning = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");
        var afternoon = await CreateShift(location.Id, "2026-01-05T13:00:00", "2026-01-05T17:00:00");

        // Act
        var response = await PutShift(afternoon.Id, location.Id, "2026-01-05T11:00:00", "2026-01-05T17:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal($"Shift overlaps with shift {morning.Id} from 2026-01-05 08:00 to 2026-01-05 12:00.", problem!.Detail);
        var fetched = await _client.GetFromJsonAsync<Shift>($"/api/shifts/{afternoon.Id}");
        Assert.Equal(new DateTime(2026, 1, 5, 13, 0, 0), fetched!.StartTime);
    }

    [Fact]
    public async Task Update_WithinItsOwnTime_Returns204()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PutShift(shift.Id, location.Id, "2026-01-05T09:00:00", "2026-01-05T12:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_ToTouchAnotherShift_Returns204()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");
        var afternoon = await CreateShift(location.Id, "2026-01-05T13:00:00", "2026-01-05T17:00:00");

        // Act
        var response = await PutShift(afternoon.Id, location.Id, "2026-01-05T12:00:00", "2026-01-05T17:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
