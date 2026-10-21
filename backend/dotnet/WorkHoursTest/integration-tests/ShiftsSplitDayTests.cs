using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;
using WorkHoursRest.Dtos;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user who works split days, I want to record more than one shift on the same date,
// so that time off between shifts (for example a free afternoon) is not counted as work.
//
// Shifts may not overlap, so no time is counted twice. A shift belongs to the date it starts on.
public class ShiftsSplitDayTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ShiftsSplitDayTests()
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

    // Several shifts on the same date

    [Fact]
    public async Task Create_TwoShiftsOnSameDate_BothReturn201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var morning = await PostShift(location.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00");
        var evening = await PostShift(location.Id, "2026-01-05T18:00:00", "2026-01-05T22:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, morning.StatusCode);
        Assert.Equal(HttpStatusCode.Created, evening.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Equal(2, all!.Count);
    }

    [Fact]
    public async Task Create_ShiftsOnSameDateAtDifferentLocations_BothReturn201()
    {
        // Arrange
        var office = await CreateLocation("Office");
        var cafe = await CreateLocation("Cafe");

        // Act
        var morning = await PostShift(office.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00");
        var evening = await PostShift(cafe.Id, "2026-01-05T18:00:00", "2026-01-05T22:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, morning.StatusCode);
        Assert.Equal(HttpStatusCode.Created, evening.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Contains(all!, s => s.LocationId == office.Id);
        Assert.Contains(all!, s => s.LocationId == cafe.Id);
    }

    [Fact]
    public async Task Create_ManyShiftsOnSameDate_AllReturn201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act: ten one-hour shifts with an hour off between each
        for (int hour = 0; hour < 20; hour += 2)
        {
            var response = await PostShift(location.Id, $"2026-01-05T{hour:D2}:00:00", $"2026-01-05T{hour + 1:D2}:00:00");
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Assert
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Equal(10, all!.Count);
    }

    // Back-to-back shifts

    [Fact]
    public async Task Create_ShiftStartingWhenAnotherEnds_Returns201()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PostShift(location.Id, "2026-01-05T12:00:00", "2026-01-05T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShiftEndingWhenAnotherStarts_Returns201()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T12:00:00", "2026-01-05T16:00:00");

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // Worked time for a date

    [Fact]
    public async Task GetDay_SplitDay_GapIsNotCounted()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00");
        await CreateShift(location.Id, "2026-01-05T18:00:00", "2026-01-05T22:00:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        Assert.Equal("8:00", day.Duration);
        Assert.Equal(8.0, day.DurationHours);
    }

    [Fact]
    public async Task GetDay_ShiftsAtDifferentLocations_AreAllCounted()
    {
        // Arrange
        var office = await CreateLocation("Office");
        var cafe = await CreateLocation("Cafe");
        await CreateShift(office.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00");
        await CreateShift(cafe.Id, "2026-01-05T18:00:00", "2026-01-05T22:00:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        Assert.Equal(2, day.Shifts.Count);
        Assert.Equal("8:00", day.Duration);
    }

    [Fact]
    public async Task GetDay_ReturnsShiftsInStartOrder()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T18:00:00", "2026-01-05T22:00:00");
        await CreateShift(location.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        Assert.Equal(new DateTime(2026, 1, 5, 7, 0, 0), day.Shifts[0].StartTime);
        Assert.Equal(new DateTime(2026, 1, 5, 18, 0, 0), day.Shifts[1].StartTime);
    }

    [Fact]
    public async Task GetDay_BackToBackShifts_AreCountedOnce()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");
        await CreateShift(location.Id, "2026-01-05T12:00:00", "2026-01-05T16:00:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        Assert.Equal("8:00", day.Duration);
    }

    [Fact]
    public async Task GetDay_ShiftOnOtherDate_IsNotCounted()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00");
        await CreateShift(location.Id, "2026-01-06T08:00:00", "2026-01-06T12:00:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        var shift = Assert.Single(day.Shifts);
        Assert.Equal(new DateTime(2026, 1, 5, 8, 0, 0), shift.StartTime);
        Assert.Equal("8:00", day.Duration);
    }

    [Fact]
    public async Task GetDay_NoShifts_ReturnsEmptyDayWithZeroHours()
    {
        // Act
        var response = await _client.GetAsync("/api/shifts/day/2026-01-05");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var day = await response.Content.ReadFromJsonAsync<DaySummaryDto>();
        Assert.Empty(day!.Shifts);
        Assert.Equal("0:00", day.Duration);
        Assert.Equal(0.0, day.DurationHours);
    }

    [Fact]
    public async Task GetDay_TotalIsRoundedOnceNotPerShift()
    {
        // Arrange: each shift is 0.33 hours, but together they are exactly 1 hour
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T08:20:00");
        await CreateShift(location.Id, "2026-01-05T09:00:00", "2026-01-05T09:20:00");
        await CreateShift(location.Id, "2026-01-05T10:00:00", "2026-01-05T10:20:00");

        // Act
        var day = await GetDay("2026-01-05");

        // Assert
        Assert.All(day.Shifts, s => Assert.Equal(0.33, s.DurationHours));
        Assert.Equal("1:00", day.Duration);
        Assert.Equal(1.0, day.DurationHours);
    }

    [Fact]
    public async Task GetDay_InvalidDate_Returns400()
    {
        // Act
        var response = await _client.GetAsync("/api/shifts/day/not-a-date");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetDay_ResponseJson_UsesDateDurationAndDurationHoursProperties()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00");
        await CreateShift(location.Id, "2026-01-05T18:00:00", "2026-01-05T22:00:00");

        // Act
        var json = await _client.GetStringAsync("/api/shifts/day/2026-01-05");

        // Assert
        Assert.Contains("\"date\":\"2026-01-05\"", json);
        Assert.Contains("\"duration\":\"8:00\"", json);
        Assert.Contains("\"durationHours\":8", json);
    }
}
