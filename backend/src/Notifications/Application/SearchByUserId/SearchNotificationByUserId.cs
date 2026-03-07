using MediatR;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Src.Notifications.Application.SearchByUserId;

public record SearchNotificationByUserIdQuery(long UserId) : IRequest<IEnumerable<NotificationResponse>?>;

public sealed class SearchNotificationByUserIdHandler : IRequestHandler<SearchNotificationByUserIdQuery,
    IEnumerable<NotificationResponse>?>
{
    private readonly INotificationRepository _repository;

    public SearchNotificationByUserIdHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<NotificationResponse>?> Handle(SearchNotificationByUserIdQuery request,
        CancellationToken cancellationToken)
    {
        var notifications = await _repository.ByUserId(request.UserId, cancellationToken);

        var result = notifications?.Select(x => new NotificationResponse(x.Id,
            x.Title,
            x.Description,
            x.Location,
            x.Price?.CurrentPrice ?? 0,
            x.Price?.PreviousPrice,
            x.Url.Value,
            x.CreatedAt,
            x.Images)).ToList();

        return result;
    }
}
