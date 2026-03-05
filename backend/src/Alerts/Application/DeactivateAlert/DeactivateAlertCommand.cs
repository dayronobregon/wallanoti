using MediatR;

namespace Wallanoti.Src.Alerts.Application.DeactivateAlert;

public sealed record DeactivateAlertCommand(Guid AlertId, long UserId) : IRequest;

