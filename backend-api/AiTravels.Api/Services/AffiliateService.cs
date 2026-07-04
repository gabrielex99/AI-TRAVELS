using AiTravels.Api.Models;

namespace AiTravels.Api.Services;

/// <summary>
/// Generates affiliate links for activities in an itinerary.
/// </summary>
public class AffiliateService
{
    private readonly string _gygPartnerId;
    private readonly string _gygBaseUrl;
    private readonly ILogger<AffiliateService> _logger;

    public AffiliateService(IConfiguration configuration, ILogger<AffiliateService> logger)
    {
        _gygPartnerId = configuration["AFFILIATE_GYG_PARTNER_ID"] ?? "default-partner";
        _gygBaseUrl = configuration["AFFILIATE_GYG_BASE_URL"] ?? "https://www.getyourguide.com";
        _logger = logger;
    }

    /// <summary>
    /// Generates affiliate links for all activities that have a GYG search term.
    /// </summary>
    /// <param name="itinerary">The itinerary to extract search terms from.</param>
    /// <returns>Dictionary mapping activity name to affiliate URL.</returns>
    public Dictionary<string, string> GenerateAffiliateLinks(ItineraryResponse itinerary)
    {
        var affiliateLinks = new Dictionary<string, string>();

        if (itinerary.Days is null)
        {
            return affiliateLinks;
        }

        foreach (var day in itinerary.Days)
        {
            if (day.DaySlots?.Slots is null)
            {
                continue;
            }

            foreach (var slot in day.DaySlots.Slots)
            {
                if (slot.Options is null)
                {
                    continue;
                }

                foreach (var activity in slot.Options)
                {
                    if (string.IsNullOrWhiteSpace(activity.GygSearchTerm))
                    {
                        continue;
                    }

                    if (affiliateLinks.ContainsKey(activity.Name))
                    {
                        continue;
                    }

                    var encodedSearch = Uri.EscapeDataString(activity.GygSearchTerm);
                    var affiliateUrl =
                        $"{_gygBaseUrl}/s/?q={encodedSearch}&partner_id={_gygPartnerId}&utm_medium=online_publisher";

                    affiliateLinks[activity.Name] = affiliateUrl;
                }
            }
        }

        _logger.LogInformation(
            "Generated {Count} affiliate links for destination: {Destination}",
            affiliateLinks.Count, itinerary.Destination);

        return affiliateLinks;
    }
}
