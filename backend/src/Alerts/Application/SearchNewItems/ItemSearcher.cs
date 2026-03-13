using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Alerts.Domain.Repositories;
using Wallanoti.Src.Alerts.Domain.Entities;
using System.Linq;

namespace Wallanoti.Src.Alerts.Application.SearchNewItems;

public sealed class ItemSearcher
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _alertRepository;
    private readonly IWallapopRepository _wallapopRepository;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly long? _ownerChatId;
    private readonly IProcessedItemRepository _processedItemRepository;
    private readonly TimeProvider _timeProvider;

    public ItemSearcher(
        IEventBus eventBus, IAlertRepository alertRepository,
        IWallapopRepository wallapopRepository,
        IPushNotificationSender pushNotificationSender,
        IConfiguration configuration,
        TimeProvider timeProvider,
        IProcessedItemRepository processedItemRepository)
    {
        _eventBus = eventBus;
        _alertRepository = alertRepository;
        _wallapopRepository = wallapopRepository;
        _pushNotificationSender = pushNotificationSender;
        _ownerChatId = long.TryParse(configuration["Notifications:OwnerChatId"], out var ownerChatId)
            ? ownerChatId
            : null;
        _timeProvider = timeProvider;
        _processedItemRepository = processedItemRepository;
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

            var candidateItems = wallapopItems
                .Where(item => DateTimeOffset.FromUnixTimeMilliseconds(item.CreatedAt).DateTime.CompareTo(alert.CreatedAt) >= 0)
                .ToList();

            var events = new List<DomainEvent>();
            var changedItems = new List<Item>();
            var now = _timeProvider.GetUtcNow();

            foreach (var item in candidateItems)
            {
                var existingProcessed = await _processedItemRepository.GetByAlertAndItemAsync(alert.Id, item.Id);
                ProcessedItem processed;
                if (existingProcessed is null)
                {
                    processed = new ProcessedItem(alert.Id, item.Id, item.ModifiedAt, item.Price!);
                    var changeEvent = new ItemChangesFoundEvent(
                        Guid.NewGuid().ToString(),
                        now.ToString("O"),
                        alert.Id,
                        item.Id,
                        ChangeType.New,
                        item);
                    events.Add(changeEvent);
                    changedItems.Add(item);
                }
                else
                {
                    existingProcessed.UpdateFrom(item);
                    var pulledEvents = existingProcessed.PullDomainEvents();
                    if (pulledEvents.Any())
                    {
                        events.AddRange(pulledEvents);
                        changedItems.Add(item);
                    }
                    processed = existingProcessed;
                }
                await _processedItemRepository.UpsertAsync(processed);
            }

            if (changedItems.Any())
            {
                alert.NewSearch(changedItems, now.UtcDateTime, now.UtcDateTime);
                await _alertRepository.Update(alert);
                await _eventBus.Publish(events);
            }
            else
            {
                alert.RecordSearch(now.UtcDateTime);
                await _alertRepository.UpdateLastSearchedAt(alert.Id, now.UtcDateTime);
            }
        }
    }




}
