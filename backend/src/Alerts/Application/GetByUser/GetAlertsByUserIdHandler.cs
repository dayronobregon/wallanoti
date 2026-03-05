using MediatR;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Src.Alerts.Application.GetByUser;

public record GetAlertsByUserIdQuery(long UserId) : IRequest<List<GetAlertsByUserIdResponse>>;

public sealed class GetAlertsByUserIdHandler : IRequestHandler<GetAlertsByUserIdQuery, List<GetAlertsByUserIdResponse>>
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

        return alerts.Select(x => new GetAlertsByUserIdResponse(x.Id, x.Name, x.Url.Value, x.IsActive)).ToList();
    }
}

public record GetAlertsByUserIdResponse(Guid Id, string Name, string Url, bool IsActive);