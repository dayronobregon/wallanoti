namespace WallapopNotification.Alerts._1_Domain.Models;

public sealed class Price
{
    public required double CurrentPrice { get; init; }
    public required string Currency { get; init; }

    public double? PreviousPrice { get; init; }
}