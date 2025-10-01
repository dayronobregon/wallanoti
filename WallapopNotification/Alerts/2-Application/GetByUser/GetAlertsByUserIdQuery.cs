using MediatR;

namespace WallapopNotification.Alerts._2_Application.GetByUser;

public record class GetAlertsByUserIdQuery(long UserId) : IRequest<List<GetAlertsByUserIdResponse>>;