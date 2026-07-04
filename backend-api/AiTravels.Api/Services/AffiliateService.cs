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
    /// <returns>Dictionary mapping GYG search term to affiliate URL.</returns>
    public Dictionary<string, string> GenerateAffiliateLinks(ItineraryResponse itinerary)
    {
        var affiliateLinks = new Dictionary<string, string>();

        if (itinerary.Itinerary is null)
        {
            return affiliateLinks;
        }

        void AddLink(ActivityOption option)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.GygSearchTerm))
            {
                return;
            }

            if (affiliateLinks.ContainsKey(option.GygSearchTerm))
            {
                return;
            }

            var encodedSearch = Uri.EscapeDataString(option.GygSearchTerm);
            var affiliateUrl = $"{_gygBaseUrl}/s/?q={encodedSearch}&partner_id={_gygPartnerId}&utm_medium=online_publisher";
            
            affiliateLinks[option.GygSearchTerm] = affiliateUrl;
        }

        foreach (var day in itinerary.Itinerary)
        {
            if (day.Slots == null)
            {
                continue;
            }

            // Morning slots
            if (day.Slots.Morning != null)
            {
                AddLink(day.Slots.Morning.OptionA);
                AddLink(day.Slots.Morning.OptionB);
            }

            // Afternoon slots
            if (day.Slots.Afternoon != null)
            {
                AddLink(day.Slots.Afternoon.OptionA);
                AddLink(day.Slots.Afternoon.OptionB);
            }

            // Evening slots
            if (day.Slots.Evening != null)
            {
                AddLink(day.Slots.Evening.OptionA);
                AddLink(day.Slots.Evening.OptionB);
            }
        }

        _logger.LogInformation(
            "Generated {Count} affiliate links for destination: {Destination}",
            affiliateLinks.Count, itinerary.Destination);

        return affiliateLinks;
    }
}
