using Bogus;
using WallapopNotification.Alerts._1_Domain.Models;

namespace WallapopNotification.Tests.Alerts._1_Domain;

public sealed class PriceFaker:Faker<Price>
{
    // public required double CurrentPrice { get; init; }
    // public required string Currency { get; init; }
    //
    // public double? PreviousPrice { get; init; }
    public PriceFaker()
    {
        RuleFor(x => x.CurrentPrice, f => f.Random.Double(0, 1000));
        RuleFor(x => x.Currency, f => f.Finance.Currency().Code);
        RuleFor(x => x.PreviousPrice, f => f.Random.Double(0, 1000));
    }
}