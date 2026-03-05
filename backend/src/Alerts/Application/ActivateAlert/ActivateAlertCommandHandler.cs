using MediatR;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Src.Alerts.Application.ActivateAlert;

public sealed class ActivateAlertCommandHandler : IRequestHandler<ActivateAlertCommand>
{
    private readonly IAlertRepository _alertRepository;

    public ActivateAlertCommandHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task Handle(ActivateAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.SearchById(request.AlertId, request.UserId);

        if (alert == null)
        {
            throw new InvalidOperationException($"Alert with ID {request.AlertId} not found");
        }

        // Desactivar todas las alertas activas del usuario
        //TODO mejorar para que sea una sola llamada
        var userAlerts = await _alertRepository.GetByUserId(request.UserId);
        foreach (var userAlert in userAlerts.Where(a => a.IsActive && a.Id != request.AlertId))
        {
            userAlert.Deactivate();
            await _alertRepository.Update(userAlert);
        }

        // Activar la alerta solicitada
        alert.Activate();
        
        await _alertRepository.Update(alert);
    }
}