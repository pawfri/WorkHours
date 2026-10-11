using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkHoursLib.Models;
using WorkHoursRest.Dtos;

namespace WorkHoursTest.IntegrationTests;

// User story: As a user, I want to record when I started and stopped working and where,
// so that my working time is tracked.
//
// Times are local wall-clock times without a time zone or offset.
public class ShiftsCreateTests : IDisposable
{
    // A new factory per test gives each test its own in-memory repository.
    private readonly WebApplicationFactory<Program> _factory = new();
    private readonly HttpClient _client;

    public ShiftsCreateTests()
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

    private Task<HttpResponseMessage> PostRawShift(string json)
    {
        return _client.PostAsync("/api/shifts", new StringContent(json, Encoding.UTF8, "application/json"));
    }

    private Task<HttpResponseMessage> PostShift(int locationId, string startTime, string endTime, string? note = null)
    {
        var noteJson = note == null ? "" : $",\"note\":\"{note}\"";
        return PostRawShift($"{{\"startTime\":\"{startTime}\",\"endTime\":\"{endTime}\",\"locationId\":{locationId}{noteJson}}}");
    }

    [Fact]
    public async Task Create_WithAllFields_Returns201WithShift()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T15:30:00", "Early start");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(new DateTime(2026, 1, 5, 8, 0, 0), created.StartTime);
        Assert.Equal(new DateTime(2026, 1, 5, 15, 30, 0), created.EndTime);
        Assert.Equal(location.Id, created.LocationId);
        Assert.Equal("Early start", created.Note);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/Shifts/{created.Id}", response.Headers.Location.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithoutNote_Returns201WithNullNote()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();
        Assert.Null(created!.Note);
    }

    [Fact]
    public async Task Create_ShiftIsPersisted_AppearsInShiftList()
    {
        // Arrange
        var location = await CreateLocation();
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00");
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();

        // Act
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");

        // Assert
        Assert.Contains(all!, s => s.Id == created!.Id && s.LocationId == location.Id);
    }

    [Fact]
    public async Task Create_LinkReturned_ResolvesToCreatedShift()
    {
        // Arrange
        var location = await CreateLocation();
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00");

        // Act
        var fetched = await _client.GetFromJsonAsync<Shift>(response.Headers.Location);

        // Assert
        Assert.NotNull(fetched);
        Assert.Equal(new DateTime(2026, 1, 5, 8, 0, 0), fetched.StartTime);
        Assert.Equal(new DateTime(2026, 1, 5, 16, 0, 0), fetched.EndTime);
    }

    // Required fields

    [Fact]
    public async Task Create_MissingStartTime_Returns400()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostRawShift($"{{\"endTime\":\"2026-01-05T16:00:00\",\"locationId\":{location.Id}}}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingEndTime_Returns400()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostRawShift($"{{\"startTime\":\"2026-01-05T08:00:00\",\"locationId\":{location.Id}}}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingLocation_Returns400()
    {
        // Arrange
        await CreateLocation();

        // Act
        var response = await PostRawShift("{\"startTime\":\"2026-01-05T08:00:00\",\"endTime\":\"2026-01-05T16:00:00\"}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyBody_Returns400()
    {
        // Act
        var response = await PostRawShift("{}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidDate_Returns400()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "not-a-date", "2026-01-05T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_MissingField_DoesNotSaveShift()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        await PostRawShift($"{{\"startTime\":\"2026-01-05T08:00:00\",\"locationId\":{location.Id}}}");

        // Assert
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Empty(all!);
    }

    // End must be after start

    [Fact]
    public async Task Create_EndEqualToStart_Returns400()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T08:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndBeforeStart_Returns400()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T16:00:00", "2026-01-05T08:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_EndOneMinuteAfterStart_Returns201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T08:01:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShiftCrossingMidnight_Returns201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T22:00:00", "2026-01-06T06:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();
        Assert.Equal("8:00", created!.Duration);
    }

    [Fact]
    public async Task Create_EndBeforeStart_DoesNotSaveShift()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        await PostShift(location.Id, "2026-01-05T16:00:00", "2026-01-05T08:00:00");

        // Assert
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Empty(all!);
    }

    // Workplace must exist

    [Fact]
    public async Task Create_UnknownLocation_Returns400()
    {
        // Act
        var response = await PostShift(999, "2026-01-05T08:00:00", "2026-01-05T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_DeletedLocation_Returns400()
    {
        // Arrange
        var location = await CreateLocation();
        await _client.DeleteAsync($"/api/locations/{location.Id}");

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownLocation_DoesNotSaveShift()
    {
        // Act
        await PostShift(999, "2026-01-05T08:00:00", "2026-01-05T16:00:00");

        // Assert
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Empty(all!);
    }

    // Past, present and future

    [Fact]
    public async Task Create_InThePast_Returns201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2020-03-02T08:00:00", "2020-03-02T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_InThePresent_Returns201()
    {
        // Arrange
        var location = await CreateLocation();
        var now = DateTime.Now;
        var start = now.AddHours(-2).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        var end = now.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

        // Act
        var response = await PostShift(location.Id, start, end);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_InTheFuture_Returns201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2099-06-01T08:00:00", "2099-06-01T16:00:00");

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // Note

    [Fact]
    public async Task Create_NoteOf500Chars_Returns201()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00", new string('a', 500));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();
        Assert.Equal(500, created!.Note!.Length);
    }

    [Fact]
    public async Task Create_NoteOf501Chars_Returns400AndDoesNotSaveShift()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00", new string('a', 501));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var all = await _client.GetFromJsonAsync<List<Shift>>("/api/shifts");
        Assert.Empty(all!);
    }

    // Duration in the response

    [Theory]
    [InlineData("2026-01-05T08:00:00", "2026-01-05T15:30:00", "7:30", 7.5)]
    [InlineData("2026-01-05T08:00:00", "2026-01-05T16:00:00", "8:00", 8.0)]
    [InlineData("2026-01-05T08:00:00", "2026-01-05T15:05:00", "7:05", 7.08)]
    [InlineData("2026-01-05T08:00:00", "2026-01-05T15:20:00", "7:20", 7.33)]
    [InlineData("2026-01-05T08:00:00", "2026-01-05T08:45:00", "0:45", 0.75)]
    [InlineData("2026-01-05T08:00:00", "2026-01-06T09:00:00", "25:00", 25.0)]
    public async Task Create_ReturnsDurationAsHoursAndMinutesAndAsDecimal(string start, string end, string expectedDuration, double expectedHours)
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, start, end);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShiftResponseDto>();
        Assert.Equal(expectedDuration, created!.Duration);
        Assert.Equal(expectedHours, created.DurationHours);
    }

    [Fact]
    public async Task Create_ResponseJson_UsesDurationAndDurationHoursProperties()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T15:30:00");

        // Assert
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"duration\":\"7:30\"", json);
        Assert.Contains("\"durationHours\":7.5", json);
    }

    // Times are local wall-clock times: no time zone conversion

    [Fact]
    public async Task Create_TimesWithoutOffset_AreReturnedUnchanged()
    {
        // Arrange
        var location = await CreateLocation();

        // Act
        var response = await PostShift(location.Id, "2026-01-05T08:00:00", "2026-01-05T16:00:00");

        // Assert
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"startTime\":\"2026-01-05T08:00:00\"", json);
        Assert.Contains("\"endTime\":\"2026-01-05T16:00:00\"", json);
    }
}
