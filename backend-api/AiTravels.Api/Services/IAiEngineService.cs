using AiTravels.Api.Models;

namespace AiTravels.Api.Services;

/// <summary>
/// Interface for communicating with the AI engine microservice.
/// </summary>
public interface IAiEngineService
{
    /// <summary>
    /// Sends a trip request to the AI engine and returns the generated itinerary.
    /// </summary>
    /// <param name="request">The trip generation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated itinerary response.</returns>
    Task<ItineraryResponse> GenerateItineraryAsync(TripRequest request, CancellationToken cancellationToken = default);
}
