using System.Text.RegularExpressions;

namespace WallapopNotification.Shared._1_Domain.ValueObject;

public sealed class Url
{
    private const string BaseUrl= "https://es.wallapop.com";

    public required string Value { get; init; }

    public static Url Create(string url)
    {
        EnsureValidUrl(url);

        return new Url
        {
            Value = url
        };
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