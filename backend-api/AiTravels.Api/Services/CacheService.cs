using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AiTravels.Api.Data;
using AiTravels.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiTravels.Api.Services;

/// <summary>
/// Handles caching of trip itineraries using the database as a persistent cache.
/// Trips are identified by a SHA-256 hash of normalized search parameters.
/// </summary>
public class CacheService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(AppDbContext dbContext, ILogger<CacheService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Computes a deterministic SHA-256 hash from normalized trip request parameters.
    /// </summary>
    public string ComputeSearchHash(TripRequest request)
    {
        var normalizedInput = string.Join("|",
            request.Destination.Trim().ToLowerInvariant(),
            request.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            request.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            request.BudgetMax?.ToString(CultureInfo.InvariantCulture) ?? "none",
            request.NumPeople.ToString(CultureInfo.InvariantCulture));

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedInput));
        return Convert.ToHexStringLower(hashBytes);
    }

    /// <summary>
    /// Attempts to retrieve a cached trip by its search hash.
    /// </summary>
    public async Task<TripEntity?> GetCachedTripAsync(string searchHash, CancellationToken cancellationToken = default)
    {
        var trip = await _dbContext.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.SearchHash == searchHash, cancellationToken);

        if (trip is not null)
        {
            _logger.LogInformation("Cache hit for search hash: {SearchHash}", searchHash);
        }
        else
        {
            _logger.LogInformation("Cache miss for search hash: {SearchHash}", searchHash);
        }

        return trip;
    }

    /// <summary>
    /// Retrieves a trip by its unique ID.
    /// </summary>
    public async Task<TripEntity?> GetTripByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a demo trip from the database by destination or fallback to the first public trip.
    /// </summary>
    public async Task<TripEntity?> GetDemoTripAsync(string destination, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(destination))
        {
            var normalizedDestination = destination.Trim();
            var trip = await _dbContext.Trips
                .AsNoTracking()
                .Where(t => t.IsPublic && EF.Functions.ILike(t.Destination, $"%{normalizedDestination}%"))
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (trip is not null)
            {
                return trip;
            }

            // Fallback for common alias or punctuation variations such as Roma,Italy -> Rome, Italy
            var fallbackTerms = new List<string>();
            var lowerDestination = normalizedDestination.ToLowerInvariant();
            if (lowerDestination.Contains("roma"))
            {
                fallbackTerms.Add("Rome");
            }
            if (lowerDestination.Contains("italy"))
            {
                fallbackTerms.Add("Italy");
            }
            if (fallbackTerms.Count > 0)
            {
                trip = await _dbContext.Trips
                    .AsNoTracking()
                    .Where(t => t.IsPublic && fallbackTerms.Any(term => EF.Functions.ILike(t.Destination, $"%{term}%")))
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (trip is not null)
                {
                    return trip;
                }
            }
        }

        return await _dbContext.Trips
            .AsNoTracking()
            .Where(t => t.IsPublic)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a trip by its URL slug and increments the view count.
    /// </summary>
    public async Task<TripEntity?> GetTripBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var trip = await _dbContext.Trips
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsPublic, cancellationToken);

        if (trip is not null)
        {
            trip.ViewCount++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return trip;
    }

    /// <summary>
    /// Persists a new trip to the database.
    /// </summary>
    public async Task<TripEntity> SaveTripAsync(
        TripRequest request,
        ItineraryResponse itinerary,
        Dictionary<string, string> affiliateLinks,
        string searchHash,
        CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(request);

        var entity = new TripEntity
        {
            Id = Guid.NewGuid(),
            Destination = request.Destination,
            TotalDays = itinerary.TotalDays,
            BudgetMax = request.BudgetMax,
            NumPeople = request.NumPeople,
            ItineraryData = JsonSerializer.Serialize(itinerary, _jsonOptions),
            AffiliateData = JsonSerializer.Serialize(affiliateLinks, _jsonOptions),
            SearchHash = searchHash,
            IsPublic = true,
            Slug = slug,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Trips.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saved trip {TripId} for destination: {Destination} with slug: {Slug}",
            entity.Id, entity.Destination, entity.Slug);

        return entity;
    }

    /// <summary>
    /// Generates a URL-friendly slug from the trip request.
    /// </summary>
    private static string GenerateSlug(TripRequest request)
    {
        var totalDays = (request.EndDate - request.StartDate).Days;
        var sanitizedDestination = request.Destination
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");

        // Remove any non-alphanumeric characters except hyphens
        var slug = new string(sanitizedDestination
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());

        // Remove consecutive hyphens
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        slug = slug.Trim('-');

        return $"{slug}-{totalDays}d-{request.NumPeople}p-{Guid.NewGuid().ToString()[..8]}";
    }
}
