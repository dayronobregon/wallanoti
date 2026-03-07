using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Alerts.Infrastructure.Percistence.Wallapop;

public sealed class WallapopRepository : IWallapopRepository
{
    private readonly HttpClient _client;
    private const string BaseUrl = "https://api.wallapop.com/api/v3/search/section";

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}?{query}");

        request.Headers.Add("Host", "api.wallapop.com");
        request.Headers.Add("X-DeviceOS", "0");
        request.Headers.Add("Cookie", "device_id=dbe99ffa-98e8-49b6-b305-44842b309020");
        var content = new StringContent(string.Empty);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content = content;
        var response = await _client.SendAsync(request);

        var responseContent = await response.Content.ReadFromJsonAsync<WallapopResponse>();
        var wallapopItems = responseContent?.Data?.Section?.Items
                           ?? responseContent?.Data?.Section?.Payload?.Items
                           ?? [];

        return wallapopItems.Select(x => new Item
        {
            Id = x.Id,
            WallapopUserId = x.UserId,
            Title = x.Title,
            Description = x.Description,
            CategoryId = x.CategoryId,
            Price = Price.Create(x.Price?.Amount ?? 0, x.Discount?.PreviousPrice?.Amount ?? x.PreviousPrice?.Amount),
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
}
