using MediatR;
using WallapopNotification.Alerts._1_Domain;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alerts._2_Application.CreateAlert;

public sealed class AlertCommandHanlder : IRequestHandler<CreateAlertCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _repository;

    public AlertCommandHanlder(IEventBus eventBus, IAlertRepository repository)
    {
        _eventBus = eventBus;
        _repository = repository;
    }

    public async Task Handle(CreateAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = Alert.Create(request.UserId, request.AlertName, request.AlertUrl);

        await _repository.Add(alert);

        await _eventBus.Publish(alert.PullDomainEvents());
    }
}