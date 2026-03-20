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

            var cacheKey = alert.GetCacheKey();
            var cachedItems = GetCachedItems(cacheKey);
            var cachedPrices = GetCachedPrices(cacheKey);

            var nonCachedItems = (from item in wallapopItems
                let itemCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime
                where itemCreatedAt.CompareTo(alert.CreatedAt) >= 0 &&
                      !AlreadyFound(cachedItems, item.Id)
                select item).ToList();

            var newItems = nonCachedItems
                .Where(item => IsEligible(item, cachedPrices))
                .ToList();

            UpdateCacheWithLatestItems(cachedItems, cachedPrices, wallapopItems);
            await SaveCache(cacheKey, cachedItems, cachedPrices);

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            if (newItems.Count == 0)
            {
                alert.RecordSearch(now);
                await _alertRepository.UpdateLastSearchedAt(alert.Id, alert.LastSearchedAt!.Value);
                continue;
            }

            //Añadir una nueva busqueda con los nuevos items encontrados
            alert.NewSearch(newItems, now, now);

            await _alertRepository.Update(alert);

            await _eventBus.Publish(alert.PullDomainEvents());
        }
    }

    private static bool AlreadyFound(HashSet<string> elementsInCache, string wallapopItemId)
    {
        return elementsInCache.Contains(wallapopItemId);
    }

    private static bool IsEligible(Item item, IReadOnlyDictionary<string, double> cachedPricesByItemId)
    {
        if (!cachedPricesByItemId.TryGetValue(item.Id, out var cachedPrice))
        {
            return true;
        }

        if (item.Price is null)
        {
            return false;
        }

        var isPriceDrop = item.Price.CurrentPrice < cachedPrice;

        if (isPriceDrop)
        {
            item.Price = Price.Create(item.Price.CurrentPrice, cachedPrice);
        }

        return isPriceDrop;
    }

    private HashSet<string> GetCachedItems(string alertId)
    {
        var cached = _cache.GetString(alertId);

        if (cached is null)
        {
            return [];
        }

        var ids = JsonSerializer.Deserialize<List<string>>(cached) ?? [];
        return ids.ToHashSet(StringComparer.Ordinal);
    }

    private Dictionary<string, double> GetCachedPrices(string alertId)
    {
        var cached = _cache.GetString(GetPriceCacheKey(alertId));

        return cached is null
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, double>>(cached) ?? [];
    }

    private static void UpdateCacheWithLatestItems(
        ISet<string> cachedItems,
        IDictionary<string, double> cachedPrices,
        IReadOnlyCollection<Item> wallapopItems)
    {
        foreach (var item in wallapopItems)
        {
            cachedItems.Add(item.Id);

            if (item.Price is null)
            {
                continue;
            }

            cachedPrices[item.Id] = item.Price.CurrentPrice;
        }
    }

    private async Task SaveCache(string alertId, IReadOnlyCollection<string> itemIds, IReadOnlyDictionary<string, double> pricesByItemId)
    {
        await _cache.SetStringAsync(alertId, JsonSerializer.Serialize(itemIds));
        await _cache.SetStringAsync(GetPriceCacheKey(alertId), JsonSerializer.Serialize(pricesByItemId));
    }

    private static string GetPriceCacheKey(string alertId)
    {
        return $"{alertId}:prices";
    }
}
