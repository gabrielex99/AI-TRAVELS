using System.Text.Json.Serialization;

namespace AiTravels.Api.Models;

/// <summary>
/// Represents a single activity option within a time slot.
/// </summary>
public record ActivityOption(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("gyg_search_term")] string GygSearchTerm
);

/// <summary>
/// Represents a time slot within a day (e.g., morning, afternoon, evening).
/// </summary>
public record TimeSlot(
    [property: JsonPropertyName("option_a")] ActivityOption OptionA,
    [property: JsonPropertyName("option_b")] ActivityOption OptionB
);

/// <summary>
/// Represents the collection of time slots for a single day.
/// </summary>
public record DaySlots(
    [property: JsonPropertyName("morning")] TimeSlot Morning,
    [property: JsonPropertyName("afternoon")] TimeSlot Afternoon,
    [property: JsonPropertyName("evening")] TimeSlot Evening
);

/// <summary>
/// Represents a single day in the itinerary.
/// </summary>
public record DayPlan(
    [property: JsonPropertyName("day")] int Day,
    [property: JsonPropertyName("theme")] string Theme,
    [property: JsonPropertyName("slots")] DaySlots Slots
);

/// <summary>
/// Parameters for flight search widget.
/// </summary>
public record FlightWidgetParams(
    [property: JsonPropertyName("destination_iata")] string DestinationIata,
    [property: JsonPropertyName("suggested_months")] List<string> SuggestedMonths
);

/// <summary>
/// The full itinerary response from the AI engine.
/// </summary>
public record ItineraryResponse(
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("total_days")] int TotalDays,
    [property: JsonPropertyName("flight_widget_params")] FlightWidgetParams FlightWidgetParams,
    [property: JsonPropertyName("itinerary")] List<DayPlan> Itinerary
);

/// <summary>
/// Enriched itinerary response with affiliate links added by the backend.
/// </summary>
public record EnrichedItineraryResponse(
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("total_days")] int TotalDays,
    [property: JsonPropertyName("flight_widget_params")] FlightWidgetParams FlightWidgetParams,
    [property: JsonPropertyName("itinerary")] List<DayPlan> Itinerary,
    [property: JsonPropertyName("affiliate_links")] Dictionary<string, string> AffiliateLinks,
    [property: JsonPropertyName("trip_id")] Guid TripId,
    [property: JsonPropertyName("slug")] string? Slug
);
