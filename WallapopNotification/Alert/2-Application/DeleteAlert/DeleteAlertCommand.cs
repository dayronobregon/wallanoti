using MediatR;
using WallapopNotification.Alert._1_Domain.Repositories;

namespace WallapopNotification.Alert._2_Application.DeleteAlert;

public record DeleteAlertCommandRequest(Guid AlertId) : IRequest;

public sealed class DeleteAlertCommand : IRequestHandler<DeleteAlertCommandRequest>
{
    private readonly IAlertRepository _alertRepository;

    public DeleteAlertCommand(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public Task Handle(DeleteAlertCommandRequest request, CancellationToken cancellationToken)
    {
        return _alertRepository.Delete(request.AlertId);
    }
}