using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Alert._1_Domain.Repositories;
using WallapopNotification.Alert._3_Infraestructure.Percistence.Wallapop.Responses;
using Location = WallapopNotification.Alert._1_Domain.Models.Location;

namespace WallapopNotification.Alert._3_Infraestructure.Percistence.Wallapop.Repositories;

public sealed class WallapopRepository : IWallapopRepository
{
    private readonly HttpClient _client;
    private const string BaseUrl = "https://api.wallapop.com/api/v3/search";

    public WallapopRepository()
    {
        _client = new HttpClient();
    }

    public async Task<List<Item>?> Latest(string url)
    {
        var uriBuilder = new UriBuilder(url);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["time_filter"] = "today";

        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}?{query}");

        request.Headers.Add("Host", "api.wallapop.com");
        request.Headers.Add("X-DeviceOS", "0");
        request.Headers.Add("Cookie", "device_id=dbe99ffa-98e8-49b6-b305-44842b309020");
        var content = new StringContent(string.Empty);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content = content;
        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error");
        }

        var responseContent = await response.Content.ReadFromJsonAsync<WallapopResponse>();

        return responseContent?.Data.Section.Payload.Items.Select(x => new Item
        {
            Id = x.Id,
            WallapopUserId = x.UserId,
            Title = x.Title,
            Description = x.Description,
            CategoryId = x.CategoryId,
            Price = new Price()
            {
                CurrentPrice = x.Price.Amount,
                Currency = x.Price.Currency,
                PreviousPrice = x.Discount?.PreviousPrice.Amount
            },
            Images = x.Images?.Select(image => image.Urls.Medium).ToList(),
            Reserved = x.Reserved.Flag,
            Location = Location.Create(x.Location.City, x.Location.Region),
            Shipping = x.Shipping.ItemIsShippable,
            Favorited = x.Favorited.Flag,
            WebSlug = x.WebSlug,
            CreatedAt = x.CreatedAt,
            ModifiedAt = x.ModifiedAt
        }).ToList();
    }
}