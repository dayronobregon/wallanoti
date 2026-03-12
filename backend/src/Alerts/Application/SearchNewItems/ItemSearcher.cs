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

            var newItems = (from item in wallapopItems
                let itemCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime
                where itemCreatedAt.CompareTo(alert.CreatedAt) >= 0 &&
                      !AlreadyFound(cachedItems, item.Id)
                select item).ToList();

            if (newItems.Count == 0)
            {
                alert.RecordSearch(_timeProvider);
                await _alertRepository.UpdateLastSearchedAt(alert.Id, alert.LastSearchedAt!.Value);
                continue;
            }

            //Añadir una nueva busqueda con los nuevos items encontrados
            alert.NewSearch(newItems, _timeProvider);

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
