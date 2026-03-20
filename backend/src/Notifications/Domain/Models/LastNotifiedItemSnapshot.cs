namespace Wallanoti.Src.Notifications.Domain.Models;

public sealed record LastNotifiedItemSnapshot(
    string Url,
    double LastNotifiedCurrentPrice,
    DateTime NotifiedAt);
