using MediatR;
using WallapopNotification.Alert._1_Domain.Repositories;

namespace WallapopNotification.Alert._2_Application.ListAlerts;

public sealed class
    GetAlertsByUserIdHandler : IRequestHandler<GetAlertsByUserIdQuery, List<GetAlertsByUserIdResponse>>
{
    private readonly IAlertRepository _alertRepository;

    public GetAlertsByUserIdHandler(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    public async Task<List<GetAlertsByUserIdResponse>> Handle(GetAlertsByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var alerts = await _alertRepository.GetByUserId(request.UserId);

        return alerts.Select(x => new GetAlertsByUserIdResponse(x.Id, x.Name)).ToList();
    }
}