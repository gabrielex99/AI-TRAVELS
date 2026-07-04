using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AiTravels.Api.Models;

/// <summary>
/// DTO for incoming trip generation requests.
/// </summary>
public class TripRequest
{
    [Required(ErrorMessage = "Destination is required.")]
    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required.")]
    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required.")]
    [JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("budget_max")]
    [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value.")]
    public decimal? BudgetMax { get; set; }

    [JsonPropertyName("num_people")]
    [Range(1, 100, ErrorMessage = "Number of people must be between 1 and 100.")]
    public int NumPeople { get; set; } = 1;
}
