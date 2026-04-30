using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Alerts.Domain.Services;

namespace Wallanoti.Src.Alerts.Infrastructure.Services;

public sealed class WallapopUrlBuilder : IWallapopUrlBuilder
{
    private const string BaseUrl = "https://es.wallapop.com/items";

    public string BuildUrl(NerEntities entities)
    {
        var parameters = new List<string>();

        // Keywords (required) - deduplicate
        if (entities.Keywords.Count > 0)
        {
            var uniqueKeywords = entities.Keywords.Distinct().ToList();
            // Use + for space encoding in keywords (URL convention for query strings)
            var encodedKeywords = string.Join("+", uniqueKeywords.Select(k => Uri.EscapeDataString(k).Replace("%20", "+")));
            parameters.Add($"keywords={encodedKeywords}");
        }

        // Brand (optional)
        if (!string.IsNullOrWhiteSpace(entities.Brand))
        {
            parameters.Add($"brand={Uri.EscapeDataString(entities.Brand)}");
        }

        // Max price (optional)
        if (entities.MaxPrice.HasValue)
        {
            parameters.Add($"max_price={entities.MaxPrice.Value.ToString("F2")}");
        }

        // Min price (optional)
        if (entities.MinPrice.HasValue)
        {
            parameters.Add($"min_price={entities.MinPrice.Value.ToString("F2")}");
        }

        // Category (optional)
        if (!string.IsNullOrWhiteSpace(entities.Category))
        {
            parameters.Add($"category_id={Uri.EscapeDataString(entities.Category)}");
        }

        // Location (optional)
        if (!string.IsNullOrWhiteSpace(entities.Location))
        {
            parameters.Add($"location={Uri.EscapeDataString(entities.Location)}");
        }

        return parameters.Count > 0
            ? $"{BaseUrl}?{string.Join("&", parameters)}"
            : BaseUrl;
    }
}
