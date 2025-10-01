using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using WallapopNotification.Alerts._1_Domain;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alerts._2_Application.SearchNewItems;

public sealed class ItemSearcher
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _alertRepository;
    private readonly IWallapopRepository _wallapopRepository;
    private readonly IDistributedCache _cache;

    public ItemSearcher(
        IEventBus eventBus, IAlertRepository alertRepository,
        IWallapopRepository wallapopRepository,
        IDistributedCache cache)
    {
        _eventBus = eventBus;
        _alertRepository = alertRepository;
        _wallapopRepository = wallapopRepository;
        _cache = cache;
    }

    public async Task Execute()
    {
        var alerts = await _alertRepository.All();

        foreach (var alert in alerts)
        {
            var wallapopItems = await _wallapopRepository.Latest(alert.Url);

            if (wallapopItems is null)
            {
                continue;
            }

            var cachedItems = GetCachedItems(alert.GetCacheKey());

            var newItems = (from item in wallapopItems
                let createdAt = DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime
                let modifiedAt = DateTimeOffset.FromUnixTimeMilliseconds(item.ModifiedAt).DateTime
                where (createdAt.CompareTo(alert.CreatedAt) >= 0 || modifiedAt.CompareTo(alert.CreatedAt) >= 0) &&
                      !AlreadyFound(cachedItems, item.Id)
                select item).ToList();

            if (newItems.Count == 0)
            {
                alert.Touch();
                continue;
            }

            //Añadir una nueva busqueda con los nuevos items encontrados
            alert.NewSearch(newItems);

            //Guardar en cache los ids de los items encontrados
            await _cache.SetAsync(alert.GetCacheKey(),
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(wallapopItems.Select(x => x.Id))));

            await _alertRepository.Update(alert);

            await _eventBus.Publish(alert.PullDomainEvents());
        }
    }

    private static bool AlreadyFound(List<string> elementsInCache, string wallapopItemId)
    {
        return elementsInCache.Count != 0 && elementsInCache.Contains(wallapopItemId);
    }

    private List<string> GetCachedItems(string alertId)
    {
        var cached = _cache.GetString(alertId);

        return cached is null ? [] : JsonSerializer.Deserialize<List<string>>(cached) ?? [];
    }
}