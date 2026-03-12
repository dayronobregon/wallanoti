using MediatR;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Src.Alerts.Application.DeactivateAlert;

public sealed class DeactivateAlertCommandHandler : IRequestHandler<DeactivateAlertCommand>
{
    private readonly IAlertRepository _alertRepository;
    private readonly TimeProvider _timeProvider;

    public DeactivateAlertCommandHandler(IAlertRepository alertRepository, TimeProvider timeProvider)
    {
        _alertRepository = alertRepository;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeactivateAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.SearchById(request.AlertId, request.UserId);

        if (alert == null)
        {
            throw new InvalidOperationException($"Alert with ID {request.AlertId} not found");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        alert.Deactivate(now);
        await _alertRepository.Update(alert);
    }
}

