namespace WallapopNotification.Alerts._1_Domain.Models;

public sealed class Location
{
    public required string FullLocation { get; init; }

    public static Location Create(string city, string region)
    {
        return new Location
        {
            FullLocation = $"{city}, {region}"
        };
    }
}