using System.Text.Json;
using AiTravels.Api.Models;
using AiTravels.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiTravels.Api.Controllers;

/// <summary>
/// Controller for itinerary generation and retrieval endpoints.
/// </summary>
[ApiController]
[Route("api/itinerary")]
public class ItineraryController : ControllerBase
{
    private readonly IAiEngineService _aiEngineService;
    private readonly AffiliateService _affiliateService;
    private readonly CacheService _cacheService;
    private readonly ILogger<ItineraryController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ItineraryController(
        IAiEngineService aiEngineService,
        AffiliateService affiliateService,
        CacheService cacheService,
        ILogger<ItineraryController> logger)
    {
        _aiEngineService = aiEngineService;
        _affiliateService = affiliateService;
        _cacheService = cacheService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Generates a new itinerary or returns a cached one.
    /// </summary>
    /// <param name="request">The trip request parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The enriched itinerary response with affiliate links.</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(EnrichedItineraryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Generate(
        [FromBody] TripRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.EndDate <= request.StartDate)
        {
            return BadRequest(new { error = "End date must be after start date." });
        }

        var totalDays = (request.EndDate - request.StartDate).Days;
        if (totalDays > 30)
        {
            return BadRequest(new { error = "Trip duration cannot exceed 30 days." });
        }

        // Step 1: Check cache
        var searchHash = _cacheService.ComputeSearchHash(request);
        var cachedTrip = await _cacheService.GetCachedTripAsync(searchHash, cancellationToken);

        if (cachedTrip is not null)
        {
            _logger.LogInformation("Returning cached itinerary for trip {TripId}", cachedTrip.Id);
            return Ok(BuildEnrichedResponse(cachedTrip));
        }

        // Step 2: Call AI engine
        ItineraryResponse itinerary;
        try
        {
            itinerary = await _aiEngineService.GenerateItineraryAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI engine request failed for destination: {Destination}", request.Destination);
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                error = "Failed to generate itinerary. The AI engine is unavailable.",
                details = ex.Message
            });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "AI engine timed out for destination: {Destination}", request.Destination);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new
            {
                error = "AI engine request timed out. Please try again.",
                details = ex.Message
            });
        }

        // Step 3: Generate affiliate links
        var affiliateLinks = _affiliateService.GenerateAffiliateLinks(itinerary);

        // Step 4: Save to cache
        var savedTrip = await _cacheService.SaveTripAsync(
            request, itinerary, affiliateLinks, searchHash, cancellationToken);

        // Step 5: Return enriched response
        var enrichedResponse = new EnrichedItineraryResponse(
            Destination: itinerary.Destination,
            TotalDays: itinerary.TotalDays,
            NumPeople: itinerary.NumPeople,
            BudgetMax: itinerary.BudgetMax,
            TotalEstimatedCost: itinerary.TotalEstimatedCost,
            Currency: itinerary.Currency,
            Days: itinerary.Days,
            FlightWidget: itinerary.FlightWidget,
            Tips: itinerary.Tips,
            AffiliateLinks: affiliateLinks,
            TripId: savedTrip.Id,
            Slug: savedTrip.Slug
        );

        return Ok(enrichedResponse);
    }

    /// <summary>
    /// Retrieves a saved trip by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnrichedItineraryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var trip = await _cacheService.GetTripByIdAsync(id, cancellationToken);

        if (trip is null)
        {
            return NotFound(new { error = $"Trip with ID '{id}' not found." });
        }

        return Ok(BuildEnrichedResponse(trip));
    }

    /// <summary>
    /// Retrieves a public trip by its URL slug.
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(EnrichedItineraryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new { error = "Slug cannot be empty." });
        }

        var trip = await _cacheService.GetTripBySlugAsync(slug, cancellationToken);

        if (trip is null)
        {
            return NotFound(new { error = $"Trip with slug '{slug}' not found or is not public." });
        }

        return Ok(BuildEnrichedResponse(trip));
    }

    /// <summary>
    /// Reconstructs an EnrichedItineraryResponse from a persisted TripEntity.
    /// </summary>
    private EnrichedItineraryResponse BuildEnrichedResponse(TripEntity trip)
    {
        var itinerary = JsonSerializer.Deserialize<ItineraryResponse>(trip.ItineraryData, _jsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize itinerary data for trip {trip.Id}.");

        var affiliateLinks = JsonSerializer.Deserialize<Dictionary<string, string>>(trip.AffiliateData, _jsonOptions)
            ?? new Dictionary<string, string>();

        return new EnrichedItineraryResponse(
            Destination: itinerary.Destination,
            TotalDays: itinerary.TotalDays,
            NumPeople: itinerary.NumPeople,
            BudgetMax: itinerary.BudgetMax,
            TotalEstimatedCost: itinerary.TotalEstimatedCost,
            Currency: itinerary.Currency,
            Days: itinerary.Days,
            FlightWidget: itinerary.FlightWidget,
            Tips: itinerary.Tips,
            AffiliateLinks: affiliateLinks,
            TripId: trip.Id,
            Slug: trip.Slug
        );
    }
}
