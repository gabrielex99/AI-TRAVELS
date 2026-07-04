using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using AiTravels.Api.Models;

namespace AiTravels.Api.Services;

/// <summary>
/// Communicates with the AI engine microservice to generate itineraries.
/// </summary>
public class AiEngineService : IAiEngineService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiEngineService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AiEngineService(HttpClient httpClient, ILogger<AiEngineService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ItineraryResponse> GenerateItineraryAsync(
        TripRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending itinerary generation request to AI engine for destination: {Destination}, " +
            "dates: {StartDate} - {EndDate}, people: {NumPeople}",
            request.Destination, request.StartDate, request.EndDate, request.NumPeople);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var payload = new
            {
                destination = request.Destination,
                start_date = request.StartDate.ToString("yyyy-MM-dd"),
                end_date = request.EndDate.ToString("yyyy-MM-dd"),
                budget_max = request.BudgetMax,
                num_people = request.NumPeople
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/generate",
                payload,
                _jsonOptions,
                cancellationToken);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "AI engine returned error status {StatusCode} after {ElapsedMs}ms: {ErrorBody}",
                    (int)response.StatusCode, stopwatch.ElapsedMilliseconds, errorBody);

                throw new HttpRequestException(
                    $"AI engine returned status {(int)response.StatusCode}: {errorBody}",
                    null,
                    response.StatusCode);
            }

            var itinerary = await response.Content.ReadFromJsonAsync<ItineraryResponse>(
                _jsonOptions, cancellationToken);

            if (itinerary is null)
            {
                throw new InvalidOperationException("AI engine returned a null itinerary response.");
            }

            _logger.LogInformation(
                "AI engine generated itinerary for {Destination} ({TotalDays} days) in {ElapsedMs}ms",
                itinerary.Destination, itinerary.TotalDays, stopwatch.ElapsedMilliseconds);

            return itinerary;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            stopwatch.Stop();
            _logger.LogError(
                "AI engine request timed out after {ElapsedMs}ms for destination: {Destination}",
                stopwatch.ElapsedMilliseconds, request.Destination);
            throw new TimeoutException(
                $"AI engine request timed out after {stopwatch.ElapsedMilliseconds}ms.", ex);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Unexpected error calling AI engine after {ElapsedMs}ms for destination: {Destination}",
                stopwatch.ElapsedMilliseconds, request.Destination);
            throw;
        }
    }
}
