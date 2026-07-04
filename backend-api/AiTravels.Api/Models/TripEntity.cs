using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiTravels.Api.Models;

/// <summary>
/// EF Core entity representing a persisted trip/itinerary in the database.
/// </summary>
[Table("trips")]
public class TripEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    [Column("destination")]
    public string Destination { get; set; } = string.Empty;

    [Column("total_days")]
    public int TotalDays { get; set; }

    [Column("budget_max")]
    public decimal? BudgetMax { get; set; }

    [Column("num_people")]
    public int NumPeople { get; set; } = 1;

    /// <summary>
    /// Serialized JSON of the full itinerary response. Stored as JSONB in PostgreSQL.
    /// </summary>
    [Column("itinerary_data", TypeName = "jsonb")]
    public string ItineraryData { get; set; } = "{}";

    /// <summary>
    /// Serialized JSON of generated affiliate links. Stored as JSONB in PostgreSQL.
    /// </summary>
    [Column("affiliate_data", TypeName = "jsonb")]
    public string AffiliateData { get; set; } = "{}";

    /// <summary>
    /// SHA-256 hash of normalized search parameters for cache lookup.
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column("search_hash")]
    public string SearchHash { get; set; } = string.Empty;

    [Column("is_public")]
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// URL-friendly slug for sharing (e.g., "rome-3-days-2-people").
    /// </summary>
    [MaxLength(500)]
    [Column("slug")]
    public string? Slug { get; set; }

    [Column("view_count")]
    public int ViewCount { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
