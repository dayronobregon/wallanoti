using Bogus;
using WallapopNotification.Alerts._1_Domain.Models;

namespace WallapopNotification.Tests.Alerts._1_Domain;

public sealed class ItemFaker:Faker<Item>
{
    public ItemFaker()
    {
        RuleFor(x => x.Id, f => f.Random.String2(10));
        RuleFor(x => x.WallapopUserId, f => f.Random.String2(10));
        RuleFor(x => x.Title, f => f.Lorem.Sentence(3));
        RuleFor(x => x.Description, f => f.Lorem.Paragraph(2));
        RuleFor(x => x.CategoryId, f => f.Random.Int(1, 1000));
        RuleFor(x => x.Price, f => new PriceFaker().Generate());
        RuleFor(x => x.Images, f => f.Make(f.Random.Int(1, 5), () => f.Image.PicsumUrl()));
        RuleFor(x => x.Reserved, f => f.Random.Bool());
        RuleFor(x => x.Shipping, f => f.Random.Bool());
        RuleFor(x => x.Favorited, f => f.Random.Bool());
        RuleFor(x => x.WebSlug, f => f.Lorem.Slug());
        RuleFor(x => x.CreatedAt, _ => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        RuleFor(x => x.ModifiedAt, _ => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}