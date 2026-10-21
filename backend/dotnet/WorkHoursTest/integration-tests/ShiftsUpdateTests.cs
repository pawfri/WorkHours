using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;
using WorkHoursRest.Dtos;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user, I want to change a shift's times, workplace or note,
// so that mistakes don't distort my hours.
//
// A shift is never compared with itself, and the totals of every affected date follow the change.
public class ShiftsUpdateTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ShiftsUpdateTests()
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

    private async Task<ShiftResponseDto> CreateShift(int locationId, string startTime, string endTime, string? note = null)
    {
        var response = await _client.PostAsJsonAsync("/api/shifts", new { startTime, endTime, locationId, note });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ShiftResponseDto>())!;
    }

    private Task<HttpResponseMessage> PutShift(int id, int locationId, string startTime, string endTime, string? note = null)
    {
        return _client.PutAsJsonAsync($"/api/shifts/{id}", new { startTime, endTime, locationId, note });
    }

    private async Task<Shift> GetShift(int id)
    {
        return (await _client.GetFromJsonAsync<Shift>($"/api/shifts/{id}"))!;
    }

    private async Task<DaySummaryDto> GetDay(string date)
    {
        return (await _client.GetFromJsonAsync<DaySummaryDto>($"/api/shifts/day/{date}"))!;
    }

    // Start, end, workplace and note can be changed

    [Fact]
    public async Task Update_AllFields_Returns204AndPersists()
    {
        // Arrange
        var office = await CreateLocation("Office");
        var cafe = await CreateLocation("Cafe");
        var shift = await CreateShift(office.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00", "Typo");

        // Act
        var response = await PutShift(shift.Id, cafe.Id, "2026-01-05T09:00:00", "2026-01-05T15:00:00", "Fixed");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await GetShift(shift.Id);
        Assert.Equal(new DateTime(2026, 1, 5, 9, 0, 0), fetched.StartTime);
        Assert.Equal(new DateTime(2026, 1, 5, 15, 0, 0), fetched.EndTime);
        Assert.Equal(cafe.Id, fetched.LocationId);
        Assert.Equal("Fixed", fetched.Note);
    }

    [Fact]
    public async Task Update_ToAnotherDate_Returns204AndPersists()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PutShift(shift.Id, location.Id, "2026-01-07T08:00:00", "2026-01-07T12:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await GetShift(shift.Id);
        Assert.Equal(new DateTime(2026, 1, 7, 8, 0, 0), fetched.StartTime);
        Assert.Equal(new DateTime(2026, 1, 7, 12, 0, 0), fetched.EndTime);
    }

    [Fact]
    public async Task Update_OmittedNote_IsCleared()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00", "Typo");

        // Act
        await PutShift(shift.Id, location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Assert
        var fetched = await GetShift(shift.Id);
        Assert.Null(fetched.Note);
    }

    [Fact]
    public async Task Update_KeepsIdAndDoesNotAddShift()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        await PutShift(shift.Id, location.Id, "2026-01-05T09:00:00", "2026-01-05T13:00:00");

        // Assert
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        var only = Assert.Single(all!);
        Assert.Equal(shift.Id, only.Id);
    }

    [Fact]
    public async Task Update_DoesNotAffectOtherShifts()
    {
        // Arrange
        var location = await CreateLocation();
        var other = await CreateShift(location.Id, "2026-01-05T14:00:00", "2026-01-05T18:00:00", "Other");
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        await PutShift(shift.Id, location.Id, "2026-01-05T07:00:00", "2026-01-05T11:00:00", "Changed");

        // Assert
        var fetched = await GetShift(other.Id);
        Assert.Equal(new DateTime(2026, 1, 5, 14, 0, 0), fetched.StartTime);
        Assert.Equal(new DateTime(2026, 1, 5, 18, 0, 0), fetched.EndTime);
        Assert.Equal("Other", fetched.Note);
    }

    // A shift is never compared with itself

    [Fact]
    public async Task Update_ExtendWithNothingInTheWay_Returns204()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PutShift(shift.Id, location.Id, "2026-01-05T08:00:00", "2026-01-05T13:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fetched = await GetShift(shift.Id);
        Assert.Equal(new DateTime(2026, 1, 5, 13, 0, 0), fetched.EndTime);
    }

    [Fact]
    public async Task Update_SameTimes_Returns204()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00", "Typo");

        // Act
        var response = await PutShift(shift.Id, location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00", "Fixed");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // Totals of every affected date are updated

    [Fact]
    public async Task Update_Times_UpdatesTotalOfDate()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        await PutShift(shift.Id, location.Id, "2026-01-05T08:00:00", "2026-01-05T13:30:00");

        // Assert
        var day = await GetDay("2026-01-05");
        Assert.Equal("5:30", day.Duration);
        Assert.Equal(5.5, day.DurationHours);
    }

    [Fact]
    public async Task Update_ToAnotherDate_UpdatesTotalsOfBothDates()
    {
        // Arrange
        var location = await CreateLocation();
        await CreateShift(location.Id, "2026-01-05T14:00:00", "2026-01-05T16:00:00");
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        await PutShift(shift.Id, location.Id, "2026-01-06T08:00:00", "2026-01-06T12:00:00");

        // Assert
        var oldDay = await GetDay("2026-01-05");
        Assert.Single(oldDay.Shifts);
        Assert.Equal("2:00", oldDay.Duration);

        var newDay = await GetDay("2026-01-06");
        var moved = Assert.Single(newDay.Shifts);
        Assert.Equal(shift.Id, moved.Id);
        Assert.Equal("4:00", newDay.Duration);
    }

    // Invalid changes are refused and leave the shift unchanged

    [Fact]
    public async Task Update_UnknownId_Returns404()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PutShift(999, location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Empty(all!);
    }

    [Theory]
    [InlineData("2026-01-05T12:00:00", "2026-01-05T08:00:00")] // end before start
    [InlineData("2026-01-05T08:00:00", "2026-01-05T08:00:00")] // end equals start
    public async Task Update_EndNotAfterStart_Returns400AndLeavesShiftUnchanged(string start, string end)
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00", "Original");

        // Act
        var response = await PutShift(shift.Id, location.Id, start, end, "Changed");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fetched = await GetShift(shift.Id);
        Assert.Equal(new DateTime(2026, 1, 5, 8, 0, 0), fetched.StartTime);
        Assert.Equal(new DateTime(2026, 1, 5, 12, 0, 0), fetched.EndTime);
        Assert.Equal("Original", fetched.Note);
    }

    [Fact]
    public async Task Update_UnknownLocation_Returns400AndLeavesShiftUnchanged()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00", "Original");

        // Act
        var response = await PutShift(shift.Id, 999, "2026-01-05T09:00:00", "2026-01-05T13:00:00", "Changed");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fetched = await GetShift(shift.Id);
        Assert.Equal(location.Id, fetched.LocationId);
        Assert.Equal(new DateTime(2026, 1, 5, 8, 0, 0), fetched.StartTime);
        Assert.Equal("Original", fetched.Note);
    }

    [Fact]
    public async Task Update_OntoAnotherShift_LeavesNoteAndLocationUnchanged()
    {
        // Arrange
        var office = await CreateLocation("Office");
        var cafe = await CreateLocation("Cafe");
        await CreateShift(office.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");
        var shift = await CreateShift(office.Id, "2026-01-05T13:00:00", "2026-01-05T17:00:00", "Original");

        // Act
        var response = await PutShift(shift.Id, cafe.Id, "2026-01-05T11:00:00", "2026-01-05T17:00:00", "Changed");

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var fetched = await GetShift(shift.Id);
        Assert.Equal(office.Id, fetched.LocationId);
        Assert.Equal("Original", fetched.Note);
    }

    [Fact]
    public async Task Update_NoteOf501Chars_Returns400()
    {
        // Arrange
        var location = await CreateLocation();
        var shift = await CreateShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00");

        // Act
        var response = await PutShift(shift.Id, location.Id, "2026-01-05T08:00:00", "2026-01-05T12:00:00",
            new string('a', Shift.NoteMaxLength + 1));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
