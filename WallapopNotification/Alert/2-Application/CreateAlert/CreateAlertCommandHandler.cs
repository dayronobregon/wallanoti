using MediatR;
using WallapopNotification.Alert._1_Domain;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Alert._1_Domain.Repositories;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.User._1_Domain.Repositories;

namespace WallapopNotification.Alert._2_Application.CreateAlert;

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
        var alert = AlertEntity.Create(request.UserId, request.AlertName, request.AlertUrl);

        await _repository.Add(alert);

        await _eventBus.Publish(alert.PullDomainEvents());
    }
}