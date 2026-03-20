using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Alerts.Application.SearchNewItems;

public sealed class ItemSearcher
{
    private const string PriceDropSuffix = "(Baja de Precio)";
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

            var cachedItems = GetCachedItems(alert.GetCacheKey());

            var eligibleItems = wallapopItems
                .Where(item => DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime.CompareTo(alert.CreatedAt) >= 0)
                .ToList();

            var newItems = new List<Item>();

            foreach (var item in eligibleItems)
            {
                var alreadyFound = AlreadyFound(cachedItems, item.Id);

                if (!alreadyFound)
                {
                    newItems.Add(item);
                    continue;
                }

                if (item.Price is null)
                {
                    continue;
                }

                if (!cachedItems.TryGetValue(item.Id, out var cachedPrice) || !cachedPrice.HasValue)
                {
                    continue;
                }

                if (item.Price.CurrentPrice < cachedPrice.Value)
                {
                    item.Title = item.Title.EndsWith(PriceDropSuffix)
                        ? item.Title
                        : $"{item.Title} {PriceDropSuffix}";
                    newItems.Add(item);
                }
            }

            UpdateCachePrices(cachedItems, eligibleItems);
            await SaveCachedItems(alert.GetCacheKey(), cachedItems);

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

    private static bool AlreadyFound(Dictionary<string, double?> elementsInCache, string wallapopItemId)
    {
        return elementsInCache.Count != 0 && elementsInCache.ContainsKey(wallapopItemId);
    }

    private Dictionary<string, double?> GetCachedItems(string alertId)
    {
        var cached = _cache.GetString(alertId);

        if (cached is null)
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, double?>>(cached) ?? [];
        }
        catch (JsonException)
        {
            var legacyCachedIds = JsonSerializer.Deserialize<List<string>>(cached) ?? [];
            return legacyCachedIds.ToDictionary(id => id, _ => (double?)null);
        }
    }

    private void UpdateCachePrices(Dictionary<string, double?> cachedItems, IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            var currentPrice = item.Price?.CurrentPrice;
            if (!currentPrice.HasValue)
            {
                continue;
            }

            cachedItems[item.Id] = currentPrice.Value;
        }
    }

    private async Task SaveCachedItems(string alertId, Dictionary<string, double?> cachedItems)
    {
        if (cachedItems.Count == 0)
        {
            return;
        }

        await _cache.SetAsync(alertId, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedItems)));
    }
}
