using System.Globalization;
using System.Text;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Notifications.Domain;

public sealed class Notification(
    Guid id,
    long userId,
    string title,
    string description,
    string location,
    Price? price,
    Url url,
    DateTime createdAt,
    List<string>? images = null)
    : AggregateRoot
{
    public Guid Id { get; } = id;
    public long UserId { get; } = userId;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public string Location { get; } = location;
    public Price? Price { get; } = price;
    public Url Url { get; } = url;
    public List<string>? Images { get; } = images;
    public DateTime CreatedAt { get; } = createdAt;

    public string FormattedString()
    {
        var message = new StringBuilder();
        var safeTitle = string.IsNullOrWhiteSpace(Title) ? "Notification" : Title;
        var safeDescription = string.IsNullOrWhiteSpace(Description) ? "No description" : Description;
        var safePrice = Price is null
            ? "N/A"
            : Price.CurrentPrice.ToString(CultureInfo.InvariantCulture);
        var safePreviousPrice = Price?.PreviousPrice?.ToString(CultureInfo.InvariantCulture);
        var safeLocation = string.IsNullOrWhiteSpace(Location) ? "Unknown" : Location;
        var hasPriceDrop = Price?.PreviousPrice.HasValue == true && Price.PreviousPrice.Value > Price.CurrentPrice;

        if (hasPriceDrop)
            message.Append("<b>Price dropped!</b>\n");

        message.Append($"<b>{safeTitle}</b>\n");
        message.Append($"<i>{safeDescription}</i>\n");
        message.Append($"<b>Price:</b> {safePrice}\n");

        if (hasPriceDrop)
            message.Append($"<b>Previous price:</b> {safePreviousPrice}\n");

        message.Append($"<b>Location:</b> {safeLocation}\n");

        if (!string.IsNullOrWhiteSpace(Url?.Value))
        {
            message.Append('\n');
            message.Append($"<a href='{Url.Value}'>{safeTitle}</a>\n");
        }

        return message.ToString();
    }

    public static Notification Create(Guid id, long userId, string title, string description, Price? price,
        List<string>? images, string location, Url url)
    {
        var now = Wallanoti.Src.Shared.Domain.AppTime.Current.GetUtcNow();

        var notification = new Notification(
            id,
            userId,
            title,
            description,
            location,
            price,
            url,
            now.UtcDateTime,
            images);

            notification.Record(new NotificationCreatedEvent(notification.Id.ToString(),
            now.ToString("o"), notification));

        return notification;
    }
}
