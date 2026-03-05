namespace Wallanoti.Src.Shared.Domain.ValueObjects;

public sealed class Price
{
    public double CurrentPrice { get; }
    public double? PreviousPrice { get; }

    public Price(double currentPrice, double? previousPrice)
    {
        CurrentPrice = currentPrice;
        PreviousPrice = previousPrice;
    }

    public static Price Create(double currentPrice, double? previousPrice)
    {
        return new Price(currentPrice, previousPrice);
    }
}