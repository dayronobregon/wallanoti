using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Alerts.Application.SearchNewItems;

public sealed class ItemSearcher
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _alertRepository;
    private readonly IWallapopRepository _wallapopRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly long? _ownerChatId;
    private readonly IDistributedCache _cache;
    private readonly TimeProvider _timeProvider;

    public ItemSearcher(
        IEventBus eventBus, IAlertRepository alertRepository,
        IWallapopRepository wallapopRepository,
        INotificationRepository notificationRepository,
        IPushNotificationSender pushNotificationSender,
        IConfiguration configuration,
        IDistributedCache cache,
        TimeProvider timeProvider)
    {
        _eventBus = eventBus;
        _alertRepository = alertRepository;
        _wallapopRepository = wallapopRepository;
        _notificationRepository = notificationRepository;
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

            var nonCachedItems = (from item in wallapopItems
                let itemCreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime
                where itemCreatedAt.CompareTo(alert.CreatedAt) >= 0 &&
                      !AlreadyFound(cachedItems, item.Id)
                select item).ToList();

            var snapshots = await GetLatestSnapshots(alert.UserId, nonCachedItems);
            var newItems = nonCachedItems
                .Where(item => IsEligible(item, snapshots))
                .ToList();

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            if (newItems.Count == 0)
            {
                alert.RecordSearch(now);
                await _alertRepository.UpdateLastSearchedAt(alert.Id, alert.LastSearchedAt!.Value);
                continue;
            }

            //Añadir una nueva busqueda con los nuevos items encontrados
            alert.NewSearch(newItems, now, now);

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

    private async Task<IReadOnlyDictionary<string, LastNotifiedItemSnapshot>> GetLatestSnapshots(long userId, List<Item> items)
    {
        if (items.Count == 0)
        {
            return new Dictionary<string, LastNotifiedItemSnapshot>();
        }

        var urls = items
            .Select(item => Url.CreateFromSlug(item.WebSlug).Value)
            .Distinct()
            .ToArray();

        return await _notificationRepository.GetLatestByUserAndUrls(userId, urls);
    }

    private static bool IsEligible(Item item, IReadOnlyDictionary<string, LastNotifiedItemSnapshot> snapshotsByUrl)
    {
        var url = Url.CreateFromSlug(item.WebSlug).Value;

        if (!snapshotsByUrl.TryGetValue(url, out var snapshot))
        {
            return true;
        }

        if (item.Price is null)
        {
            return false;
        }

        return item.Price.CurrentPrice < snapshot.LastNotifiedCurrentPrice;
    }

    private List<string> GetCachedItems(string alertId)
    {
        var cached = _cache.GetString(alertId);

        return cached is null ? [] : JsonSerializer.Deserialize<List<string>>(cached) ?? [];
    }
}
