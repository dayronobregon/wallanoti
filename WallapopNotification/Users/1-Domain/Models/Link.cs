namespace WallapopNotification.Users._1_Domain.Models;

public sealed class Link
{
    public readonly string Url;

    private Link(string url)
    {
        Url = url;
    }

    public static Link CreateFromSlug(string slug)
    {
        return new Link($"https://es.wallapop.com/item/{slug}");
    }
}