using MediatR;

namespace WallapopNotification.Alert._2_Application.ListAlerts;

public record class GetAlertsByUserIdQuery(long UserId) : IRequest<List<GetAlertsByUserIdResponse>>;