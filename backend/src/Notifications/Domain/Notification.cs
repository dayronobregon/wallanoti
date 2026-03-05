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
    Price price,
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
    public Price Price { get; } = price;
    public Url Url { get; } = url;
    public List<string>? Images { get; } = images;
    public DateTime CreatedAt { get; } = createdAt;

    public string FormattedString()
    {
        var message = new StringBuilder();
        message.Append($"<b>{Title}</b>\n");
        message.Append($"<i>{Description}</i>\n");
        message.Append($"<b>Price:</b> {Price.CurrentPrice}\n");
        message.Append($"<b>Location:</b> {Location}\n");
        message.Append('\n'); // Agregar una línea en blanco
        message.Append($"<a href='{Url.Value}'>{Title}</a>\n");

        return message.ToString();
    }

    public static Notification Create(Guid id, long userId, string title, string description, Price price,
        List<string>? images, string location, Url url)
    {
        var notification = new Notification(
            id,
            userId,
            title,
            description,
            location,
            price,
            url,
            TimeProvider.System.GetUtcNow().UtcDateTime,
            images);

        notification.Record(new NotificationCreatedEvent(notification.Id.ToString(),
            TimeProvider.System.GetUtcNow().ToString(), notification));

        return notification;
    }
}