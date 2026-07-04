using System.Text.Json.Serialization;

namespace AiTravels.Api.Models;

/// <summary>
/// Represents a single activity option within a time slot.
/// </summary>
public record ActivityOption(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("estimated_cost")] decimal EstimatedCost,
    [property: JsonPropertyName("duration_minutes")] int DurationMinutes,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("gyg_search_term")] string? GygSearchTerm = null,
    [property: JsonPropertyName("booking_url")] string? BookingUrl = null,
    [property: JsonPropertyName("rating")] double? Rating = null,
    [property: JsonPropertyName("address")] string? Address = null
);

/// <summary>
/// Represents a time slot within a day (e.g., morning, afternoon, evening).
/// </summary>
public record TimeSlot(
    [property: JsonPropertyName("time_label")] string TimeLabel,
    [property: JsonPropertyName("start_time")] string StartTime,
    [property: JsonPropertyName("end_time")] string EndTime,
    [property: JsonPropertyName("options")] List<ActivityOption> Options
);

/// <summary>
/// Represents the collection of time slots for a single day.
/// </summary>
public record DaySlots(
    [property: JsonPropertyName("slots")] List<TimeSlot> Slots
);

/// <summary>
/// Represents a single day in the itinerary.
/// </summary>
public record DayPlan(
    [property: JsonPropertyName("day_number")] int DayNumber,
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("theme")] string Theme,
    [property: JsonPropertyName("day_slots")] DaySlots DaySlots,
    [property: JsonPropertyName("daily_budget_estimate")] decimal DailyBudgetEstimate
);

/// <summary>
/// Parameters for flight search widget.
/// </summary>
public record FlightWidgetParams(
    [property: JsonPropertyName("origin")] string? Origin,
    [property: JsonPropertyName("destination_iata")] string DestinationIata,
    [property: JsonPropertyName("departure_date")] string DepartureDate,
    [property: JsonPropertyName("return_date")] string ReturnDate,
    [property: JsonPropertyName("num_passengers")] int NumPassengers
);

/// <summary>
/// The full itinerary response from the AI engine.
/// </summary>
public record ItineraryResponse(
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("total_days")] int TotalDays,
    [property: JsonPropertyName("num_people")] int NumPeople,
    [property: JsonPropertyName("budget_max")] decimal? BudgetMax,
    [property: JsonPropertyName("total_estimated_cost")] decimal TotalEstimatedCost,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("days")] List<DayPlan> Days,
    [property: JsonPropertyName("flight_widget")] FlightWidgetParams? FlightWidget = null,
    [property: JsonPropertyName("tips")] List<string>? Tips = null
);

/// <summary>
/// Enriched itinerary response with affiliate links added by the backend.
/// </summary>
public record EnrichedItineraryResponse(
    [property: JsonPropertyName("destination")] string Destination,
    [property: JsonPropertyName("total_days")] int TotalDays,
    [property: JsonPropertyName("num_people")] int NumPeople,
    [property: JsonPropertyName("budget_max")] decimal? BudgetMax,
    [property: JsonPropertyName("total_estimated_cost")] decimal TotalEstimatedCost,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("days")] List<DayPlan> Days,
    [property: JsonPropertyName("flight_widget")] FlightWidgetParams? FlightWidget,
    [property: JsonPropertyName("tips")] List<string>? Tips,
    [property: JsonPropertyName("affiliate_links")] Dictionary<string, string> AffiliateLinks,
    [property: JsonPropertyName("trip_id")] Guid TripId,
    [property: JsonPropertyName("slug")] string? Slug
);
