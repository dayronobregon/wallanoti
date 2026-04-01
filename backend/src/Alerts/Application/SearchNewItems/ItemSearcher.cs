using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Alerts.Application.SearchNewItems;

public sealed class ItemSearcher
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _alertRepository;
    private readonly IWallapopRepository _wallapopRepository;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly long? _ownerChatId;
    private readonly IDistributedCache _cache;
    private readonly TimeProvider _timeProvider;

    public ItemSearcher(
        IEventBus eventBus, IAlertRepository alertRepository,
        IWallapopRepository wallapopRepository,
        IPushNotificationSender pushNotificationSender,
        IConfiguration configuration,
        IDistributedCache cache,
        TimeProvider timeProvider)
    {
        _eventBus = eventBus;
        _alertRepository = alertRepository;
        _wallapopRepository = wallapopRepository;
        _pushNotificationSender = pushNotificationSender;
        _ownerChatId = long.TryParse(configuration["Notifications:OwnerChatId"], out var ownerChatId)
            ? ownerChatId
            : null;
        _cache = cache;
        _timeProvider = timeProvider;
    }

    public async Task Execute()
    {
        var alerts = await _alertRepository.All();

        foreach (var alert in alerts)
        {
            List<Item>? wallapopItems;

            try
            {
                wallapopItems = await _wallapopRepository.Latest(alert.Url);
            }
            catch (Exception exception)
            {
                if (_ownerChatId.HasValue)
                {
                    await _pushNotificationSender.Notify(
                        _ownerChatId.Value,
                        $"Wallanoti internal error\nSource: alerts.wallapop.latest\nError: {exception.GetType().Name}: {exception.Message}\nContext: alertId={alert.Id}; userId={alert.UserId}; url={alert.Url}");
                }

                continue;
            }

            if (wallapopItems is null)
            {
                continue;
            }

            var cachedPrices = GetCachedPrices(alert.GetCacheKey());

            var labeledItems = ClassifyItems(wallapopItems, alert.CreatedAt, cachedPrices);

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            if (labeledItems.Count == 0)
            {
                alert.RecordSearch(now);
                await _alertRepository.UpdateLastSearchedAt(alert.Id, alert.LastSearchedAt!.Value);
                continue;
            }

            alert.NewSearch(labeledItems, now, now);

            var updatedPrices = BuildUpdatedPrices(wallapopItems, cachedPrices, labeledItems);
            await _cache.SetAsync(alert.GetCacheKey(),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(updatedPrices)));

            await _alertRepository.Update(alert);

            await _eventBus.Publish(alert.PullDomainEvents());
        }
    }

    private List<LabeledAlertItem> ClassifyItems(
        List<Item> wallapopItems,
        DateTime alertCreatedAt,
        Dictionary<string, double> cachedPrices)
    {
        var labeled = new List<LabeledAlertItem>();

        foreach (var item in wallapopItems)
        {
            var itemCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime;

            if (!cachedPrices.TryGetValue(item.Id, out var cachedPrice))
            {
                // Item never seen before — only include if it was created after the alert
                if (itemCreatedAt.CompareTo(alertCreatedAt) >= 0)
                {
                    labeled.Add(new LabeledAlertItem(item, ItemNotificationLabel.New));
                }

                continue;
            }

            // Item already seen — check for price drop
            var currentPrice = item.Price?.CurrentPrice;

            if (currentPrice.HasValue && currentPrice.Value < cachedPrice)
            {
                item.Price = Price.Create(currentPrice.Value, cachedPrice);
                labeled.Add(new LabeledAlertItem(item, ItemNotificationLabel.PriceDrop));
            }

            // Same price or price increase — skip
        }

        return labeled;
    }

    private static Dictionary<string, double> BuildUpdatedPrices(
        List<Item> allItems,
        Dictionary<string, double> existingCachedPrices,
        List<LabeledAlertItem> processedItems)
    {
        // Start from current cached prices
        var updated = new Dictionary<string, double>(existingCachedPrices);

        // Update prices for items that were processed (new or price drop)
        foreach (var labeled in processedItems)
        {
            var price = labeled.Item.Price?.CurrentPrice;
            if (price.HasValue)
            {
                updated[labeled.Item.Id] = price.Value;
            }
        }

        // Also ensure ALL items from Wallapop are tracked in cache (even unchanged ones)
        // so we can detect future price drops on them
        foreach (var item in allItems)
        {
            if (!updated.ContainsKey(item.Id) && item.Price?.CurrentPrice is { } p)
            {
                updated[item.Id] = p;
            }
        }

        return updated;
    }

    private Dictionary<string, double> GetCachedPrices(string cacheKey)
    {
        var cached = _cache.GetString(cacheKey);

        if (cached is null)
            return [];

        // Try to deserialize as new schema: Dictionary<string, double>
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, double>>(cached) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
