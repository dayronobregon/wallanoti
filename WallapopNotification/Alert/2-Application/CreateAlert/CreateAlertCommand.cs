using MediatR;

namespace WallapopNotification.Alert._2_Application.CreateAlert;

public record CreateAlertCommand( long UserId, string AlertName, string AlertUrl) : IRequest;