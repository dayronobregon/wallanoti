using MediatR;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Alerts.Application.CreateAlert;

public record CreateAlertCommand(long UserId, string AlertName, string AlertUrl) : IRequest;

public sealed class AlertCommandHanlder : IRequestHandler<CreateAlertCommand>
{
    private readonly IEventBus _eventBus;
    private readonly IAlertRepository _repository;
    private readonly TimeProvider _timeProvider;

    public AlertCommandHanlder(IEventBus eventBus, IAlertRepository repository, TimeProvider timeProvider)
    {
        _eventBus = eventBus;
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task Handle(CreateAlertCommand command, CancellationToken cancellationToken)
    {
        var alert = Alert.Create(command.UserId, command.AlertName, command.AlertUrl, _timeProvider);

        await _repository.Add(alert);

        await _eventBus.Publish(alert.PullDomainEvents());
    }
}