namespace WallapopNotification.Users._1_Domain.Models;

public sealed class Price
{
    public required double CurrentPrice { get; init; }
    public double? PreviousPrice { get; init; }
}