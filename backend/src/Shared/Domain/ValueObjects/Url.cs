namespace Wallanoti.Src.Shared.Domain.ValueObjects;

public sealed class Url
{
    private const string BaseUrl = "https://es.wallapop.com";

    public required string Value { get; init; }

    public static Url Create(string url)
    {
        EnsureValidUrl(url);

        return new Url
        {
            Value = url
        };
    }

    public static Url CreateFromSlug(string slug)
    {
        var url = $"{BaseUrl}/item/{slug}";
        return Create(url);
    }

    private static void EnsureValidUrl(string url)
    {
        if (!IsValid(url))
        {
            throw new ArgumentException("Invalid url");
        }
    }

    public static bool IsValid(string url)
    {
        return url.Contains(BaseUrl);
    }

    public override string ToString()
    {
        return Value;
    }
}