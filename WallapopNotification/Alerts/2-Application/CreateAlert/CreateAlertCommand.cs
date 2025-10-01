using MediatR;

namespace WallapopNotification.Alerts._2_Application.CreateAlert;

public record CreateAlertCommand( long UserId, string AlertName, string AlertUrl) : IRequest;