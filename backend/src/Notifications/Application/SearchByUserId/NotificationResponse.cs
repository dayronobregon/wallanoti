namespace Wallanoti.Src.Notifications.Application.SearchByUserId;

public record NotificationResponse(
    Guid Id,
    string Title,
    string Description,
    string Location,
    double? CurrentPrice,
    double? PreviousPrice,
    string Url,
    DateTime CreatedAt,
    List<string>? Images = null);