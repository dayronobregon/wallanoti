using System.Text.Json.Serialization;

namespace WallapopNotification.Alerts._3_Infraestructure.Percistence.Wallapop;

public sealed class WallapopResponse
{
    public Data Data { get; set; }
}

public sealed class Data
{
    public Section Section { get; set; }
}

public class Section
{
    public Payload Payload { get; set; }
}

public class Payload
{
    public string Order { get; set; }
    public string Title { get; set; }
    public List<WallapopItem> Items { get; set; }
}

public class WallapopItem
{
    public string Id { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }

    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }

    public PriceW Price { get; set; }
    public List<Image>? Images { get; set; }
    public Reserved Reserved { get; set; }
    public Location Location { get; set; }
    public Shipping Shipping { get; set; }
    public Favorited Favorited { get; set; }
    public Bump Bump { get; set; }

    [JsonPropertyName("web_slug")]
    public string WebSlug { get; set; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("modified_at")]
    public long ModifiedAt { get; set; }

    [JsonPropertyName("discount")]
    public DiscountW? Discount { get; set; }

    public List<Taxonomy> Taxonomy { get; set; }

    [JsonPropertyName("is_favoriteable")]
    public IsFavoriteable IsFavoriteable { get; set; }

    [JsonPropertyName("is_refurbished")]
    public IsRefurbished IsRefurbished { get; set; }
}

public class PriceW
{
    public double Amount { get; set; }
    public string Currency { get; set; }
}

public class Image
{
    public string AverageColor { get; set; }
    public required Urls Urls { get; set; }
}

public class Urls
{
    public required string Small { get; set; }
    public required string Medium { get; set; }
    public required string Big { get; set; }
}

public class Reserved
{
    public bool Flag { get; set; }
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    public string City { get; set; }
    public string Region { get; set; }
    public string Region2 { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
}

public class Shipping
{
    [JsonPropertyName("shipping_price")]
    public bool ItemIsShippable { get; set; }

    [JsonPropertyName("user_allows_shipping")]
    public bool UserAllowsShipping { get; set; }

    [JsonPropertyName("cost_configuration_id")]
    public string CostConfigurationId { get; set; }
}

public class Favorited
{
    public bool Flag { get; set; }
}

public class Bump
{
    public string Type { get; set; }
}

public class DiscountW
{
    [JsonPropertyName("previous_price")]
    public PreviousPrice PreviousPrice { get; set; }

    public int Percentage { get; set; }
}

public class PreviousPrice
{
    public double Amount { get; set; }
    public string Currency { get; set; }
}

public class Taxonomy
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
}

public class IsFavoriteable
{
    public bool Flag { get; set; }
}

public class IsRefurbished
{
    public bool Flag { get; set; }
}