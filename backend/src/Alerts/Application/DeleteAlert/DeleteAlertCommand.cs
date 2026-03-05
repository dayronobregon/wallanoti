using MediatR;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Src.Alerts.Application.DeleteAlert;

public record DeleteAlertCommandRequest(Guid AlertId) : IRequest;

public sealed class DeleteAlertCommand : IRequestHandler<DeleteAlertCommandRequest>
{
    private readonly IAlertRepository _alertRepository;

    public DeleteAlertCommand(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task Handle(DeleteAlertCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await _alertRepository.Delete(request.AlertId);

        if (!result)
        {
            //TODO crear expeciones. AlertNotFoundException
            throw new Exception("Alert not found");
        }
    }
}