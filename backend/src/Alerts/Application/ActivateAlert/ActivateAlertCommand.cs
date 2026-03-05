using MediatR;

namespace Wallanoti.Src.Alerts.Application.ActivateAlert;

public sealed record ActivateAlertCommand(Guid AlertId, long UserId) : IRequest;