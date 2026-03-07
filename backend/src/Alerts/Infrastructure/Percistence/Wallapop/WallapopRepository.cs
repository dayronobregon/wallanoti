using System.Net.Http.Json;
using System.Web;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Alerts.Infrastructure.Percistence.Wallapop;

public sealed class WallapopRepository : IWallapopRepository
{
    private readonly HttpClient _client;
    private const string SectionSearchUrl = "https://api.wallapop.com/api/v3/search/section";
    private const string LegacySearchUrl = "https://api.wallapop.com/api/v3/search";

    public WallapopRepository()
        : this(new HttpClient())
    {
    }

    public WallapopRepository(HttpClient client)
    {
        _client = client;
    }

    public async Task<List<Item>?> Latest(Url url)
    {
        var uriBuilder = new UriBuilder(url.ToString());
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["time_filter"] = "today";
        query["order_by"] = "newest";
        query["section_type"] = "organic_search_results";
        query["step"] = "1";
        query["source"] = "keywords";
        query["limit"] = "40";

        var sectionSearchUri = $"{SectionSearchUrl}?{query}";
        var response = await SendSearchRequest(sectionSearchUri);

        if ((int)response.StatusCode == 400)
        {
            query.Remove("section_type");
            var legacySearchUri = $"{LegacySearchUrl}?{query}";
            response = await SendSearchRequest(legacySearchUri);
        }

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadFromJsonAsync<WallapopResponse>();
        if (responseContent?.Data is null)
            return [];

        if (responseContent.Data.Section is null)
            throw new InvalidOperationException("Unexpected Wallapop payload shape: 'data.section' is missing.");

        var wallapopItems = responseContent.Data.Section.Items
                           ?? responseContent.Data.Section.Payload?.Items
                           ?? throw new InvalidOperationException(
                               "Unexpected Wallapop payload shape: neither 'data.section.items' nor 'data.section.payload.items' is present.");

        return wallapopItems.Select(x => new Item
        {
            Id = x.Id,
            WallapopUserId = x.UserId,
            Title = x.Title,
            Description = x.Description,
            CategoryId = x.CategoryId,
            Price = x.Price is null
                ? null
                : Price.Create(x.Price.Amount, x.Discount?.PreviousPrice?.Amount ?? x.PreviousPrice?.Amount),
            Images = x.Images?.Select(image => image.Urls.Medium).ToList(),
            Reserved = x.Reserved?.Flag ?? false,
            Location = Domain.Models.Location.Create(x.Location?.City ?? string.Empty, x.Location?.Region ?? string.Empty),
            Shipping = x.Shipping?.ItemIsShippable ?? false,
            Favorited = x.Favorited?.Flag ?? false,
            WebSlug = x.WebSlug,
            CreatedAt = x.CreatedAt,
            ModifiedAt = x.ModifiedAt
        }).ToList();
    }

    private async Task<HttpResponseMessage> SendSearchRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Host", "api.wallapop.com");
        request.Headers.Add("X-DeviceOS", "0");
        return await _client.SendAsync(request);
    }
}
