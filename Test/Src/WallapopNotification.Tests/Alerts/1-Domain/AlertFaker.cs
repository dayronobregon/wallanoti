using Bogus;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._1_Domain.ValueObject;

namespace WallapopNotification.Tests.Alerts._1_Domain;

public sealed class AlertFaker : Faker<Alert>
{
    public AlertFaker()
    {
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.UserId, f => f.Random.Long(1, 1000));
        RuleFor(x => x.Name, f => f.Lorem.Sentence(3));
        RuleFor(x => x.Url, f => Url.Create(f.Internet.UrlWithPath("https", "es.wallapop.com")));
        RuleFor(x => x.CreatedAt, f => f.Date.Past());
        RuleFor(x => x.LastSearch, f => f.Date.Past());
    }
}