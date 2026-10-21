using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;
using WorkHoursRest.Dtos;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user I want to record a shift that ends after midnight as one shift,
// so that I don't have to split night work in two.
//
// A shift belongs to the date it starts on, and does not also appear on the following date.
public class ShiftsAcrossMidnightTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ShiftsAcrossMidnightTests()
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

    private Task<HttpResponseMessage> PostShift(int locationId, string startTime, string endTime)
    {
        var json = $"{{\"startTime\":\"{startTime}\",\"endTime\":\"{endTime}\",\"locationId\":{locationId}}}";
        return _client.PostAsync("/api/shifts", new StringContent(json, Encoding.UTF8, "application/json"));
    }

    private async Task CreateShift(int locationId, string startTime, string endTime)
    {
        var response = await PostShift(locationId, startTime, endTime);
        response.EnsureSuccessStatusCode();
    }

    private async Task<DaySummaryDto> GetDay(string date)
    {
        return (await _client.GetFromJsonAsync<DaySummaryDto>($"/api/shifts/day/{date}"))!;
    }

    // One shift across midnight

    [Fact]
    public async Task Create_ShiftCrossingMidnight_Returns201AsOneEightHourShift()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();
        Assert.Equal(new DateTime(2026, 1, 5, 22, 0, 0), created!.StartTime);
        Assert.Equal(new DateTime(2026, 1, 6, 6, 0, 0), created.EndTime);
        Assert.Equal("8:00", created.Duration);
        Assert.Equal(8.0, created.DurationHours);
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Single(all!);
    }

    // The shift belongs to the date it starts on

    [Fact]
    public async Task GetDay_ShiftCrossingMidnight_IsListedAndCountedOnStartDate()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        var shift = Assert.Single(day.Shifts);
        Assert.Equal(new DateTime(2026, 1, 5, 22, 0, 0), shift.StartTime);
        Assert.Equal(new DateTime(2026, 1, 6, 6, 0, 0), shift.EndTime);
        Assert.Equal("8:00", shift.Duration);
        Assert.Equal("8:00", day.Duration);
        Assert.Equal(8.0, day.DurationHours);
    }

    [Fact]
    public async Task GetDay_ShiftCrossingMidnight_DoesNotAppearOnFollowingDate()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Act
        var day = await GetDay("2026-01-06");

        // Assert
        Assert.Empty(day.Shifts);
        Assert.Equal("0:00", day.Duration);
    }

    [Fact]
    public async Task GetDay_NightShiftThenMorningShift_EachCountsOnItsOwnStartDate()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");
        await CreateShift(location.Id, "2026-01-06T08:00:00", "2026-01-06T12:00:00");

        // Act
        var firstDay = await GetDay("2026-01-05");
        var secondDay = await GetDay("2026-01-06");

        // Assert
        Assert.Single(firstDay.Shifts);
        Assert.Equal("8:00", firstDay.Duration);
        var morning = Assert.Single(secondDay.Shifts);
        Assert.Equal(new DateTime(2026, 1, 6, 8, 0, 0), morning.StartTime);
        Assert.Equal("4:00", secondDay.Duration);
    }

    // Exactly at midnight

    [Fact]
    public async Task GetDay_ShiftEndingAtMidnight_CountsOnlyOnStartDate()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T16:00:00", "2026-01-06T00:00:00");

        // Act
        var startDay = await GetDay("2026-01-05");
        var nextDay = await GetDay("2026-01-06");

        // Assert
        Assert.Single(startDay.Shifts);
        Assert.Equal("8:00", startDay.Duration);
        Assert.Empty(nextDay.Shifts);
        Assert.Equal("0:00", nextDay.Duration);
    }

    [Fact]
    public async Task GetDay_ShiftStartingAtMidnight_CountsOnThatDateNotThePreviousOne()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-06T00:00:00", "2026-01-06T08:00:00");

        // Act
        var previousDay = await GetDay("2026-01-05");
        var day = await GetDay("2026-01-06");

        // Assert
        Assert.Empty(previousDay.Shifts);
        Assert.Equal("0:00", previousDay.Duration);
        Assert.Single(day.Shifts);
        Assert.Equal("8:00", day.Duration);
    }
}
