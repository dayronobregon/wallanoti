using MediatR;
using WallapopNotification.Alert._1_Domain;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Alert._1_Domain.Repositories;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alert._2_Application.SearchAlertInWallapop;

public sealed class AlertSearcher
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _alertRepository;
    private readonly IWallapopRepository _wallapopRepository;


    public AlertSearcher(IEventBus eventBus, IAlertRepository alertRepository,
        IWallapopRepository wallapopRepository)
    {
        _eventBus = eventBus;
        _alertRepository = alertRepository;
        _wallapopRepository = wallapopRepository;
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

            alert.NewSearch(wallapopItems);

            await _alertRepository.Update(alert);

            await _eventBus.Publish(alert.PullDomainEvents());
        }
    }
}