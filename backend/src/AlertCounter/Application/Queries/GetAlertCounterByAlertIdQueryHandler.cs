using MediatR;
using Wallanoti.Src.AlertCounter.Domain;

namespace Wallanoti.Src.AlertCounter.Application.Queries;

public record GetAlertCounterByAlertIdQuery(Guid AlertId) : IRequest<Domain.AlertCounter?>;

public sealed class GetAlertCounterByAlertIdQueryHandler : IRequestHandler<GetAlertCounterByAlertIdQuery, Domain.AlertCounter?>
{
    private readonly IAlertCounterRepository _alertCounterRepository;

    public GetAlertCounterByAlertIdQueryHandler(IAlertCounterRepository alertCounterRepository)
    {
        _alertCounterRepository = alertCounterRepository;
    }

    public async Task<Domain.AlertCounter?> Handle(GetAlertCounterByAlertIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _alertCounterRepository.SearchByAlertId(request.AlertId);
    }
}