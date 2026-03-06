using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._1_Domain;

public class NotificationTest
{
    [Fact]
    public void Create_ShouldRaiseDomainEventAndSetProperties()
    {
        var id = Guid.NewGuid();
        var url = Url.CreateFromSlug("item-slug");
        var notification = Notification.Create(id, 5, "title", "description", Price.Create(50, 60),
            new List<string> { "img" }, "City, Region", url);

        Assert.Equal(id, notification.Id);
        Assert.Equal(5, notification.UserId);
        Assert.Equal("title", notification.Title);
        Assert.Equal("description", notification.Description);
        Assert.Equal("City, Region", notification.Location);
        Assert.Equal(url.Value, notification.Url.Value);

        var events = notification.PullDomainEvents();
        var createdEvent = Assert.Single(events.OfType<NotificationCreatedEvent>());
        Assert.Equal(notification, createdEvent.Notification);
    }

    [Fact]
    public void FormattedString_ShouldIncludeKeyInformation()
    {
        var notification = Notification.Create(Guid.NewGuid(), 10, "Bike", "Great condition",
            Price.Create(200, 250), new List<string>(), "Madrid", Url.CreateFromSlug("bike-slug"));

        var formatted = notification.FormattedString();

        Assert.Contains(notification.Title, formatted);
        Assert.Contains(notification.Description, formatted);
        Assert.Contains(notification.Price.CurrentPrice.ToString(), formatted);
        Assert.Contains(notification.Location, formatted);
        Assert.Contains(notification.Url.Value, formatted);
    }
}
