using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.ValueObjects;
using System.Text.Json;

namespace Wallanoti.Tests.Notifications._1_Domain;

public class NotificationTest
{
    private readonly DateTime _now;

    public NotificationTest()
    {
        _now = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
    }

    [Fact]
    public void Create_ShouldRaiseDomainEventAndSetProperties()
    {
        var id = Guid.NewGuid();
        var url = Url.CreateFromSlug("item-slug");
        var notification = Notification.Create(id, 5, "title", "description", Price.Create(50, 60),
            new List<string> { "img" }, "City, Region", url, _now);

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
            Price.Create(200, 250), new List<string>(), "Madrid", Url.CreateFromSlug("bike-slug"), _now);

        var formatted = notification.FormattedString();

        Assert.Contains(notification.Title, formatted);
        Assert.Contains(notification.Description, formatted);
        Assert.Contains(notification.Price.CurrentPrice.ToString(), formatted);
        Assert.Contains(notification.Location, formatted);
        Assert.Contains(notification.Url.Value, formatted);
    }

    [Fact]
    public void NotificationCreatedEvent_SystemTextJsonRoundtrip_ShouldPreservePriceAndUrl()
    {
        var notification = Notification.Create(Guid.NewGuid(), 10, "Bike", "Great condition",
            Price.Create(200, 250), new List<string>(), "Madrid", Url.CreateFromSlug("bike-slug"), _now);
        var createdEvent = new NotificationCreatedEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("O"),
            notification);

        var payload = JsonSerializer.Serialize(createdEvent);
        var restored = JsonSerializer.Deserialize<NotificationCreatedEvent>(payload);

        Assert.NotNull(restored);

        Assert.NotNull(restored!.Notification.Price);
        Assert.Equal(notification.Price.CurrentPrice, restored.Notification.Price.CurrentPrice);
        Assert.NotNull(restored.Notification.Url);
        Assert.Equal(notification.Url.Value, restored.Notification.Url.Value);
    }

    [Fact]
    public void FormattedString_WhenPriceOrUrlIsMissing_ShouldNotThrowAndUseFallbacks()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            10,
            "Bike",
            "Great condition",
            "Madrid",
            null!,
            null!,
            DateTime.UtcNow,
            []);

        var formatted = notification.FormattedString();

        Assert.Contains("<b>Price:</b> N/A", formatted);
        Assert.DoesNotContain("<a href='", formatted);
    }

    [Fact]
    public void FormattedString_WhenPreviousPriceIsHigher_ShouldShowPriceDroppedMessage()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            10,
            "Bike",
            "Great condition",
            Price.Create(200, 250),
            [],
            "Madrid",
            Url.CreateFromSlug("bike-slug"),
            _now);

        var formatted = notification.FormattedString();

        Assert.Contains("Price dropped", formatted);
        Assert.Contains("<b>Price:</b> 200", formatted);
        Assert.Contains("<b>Previous price:</b> 250", formatted);
    }

    [Fact]
    public void FormattedString_NewPrefix()
    {
        var notification = Notification.Create(Guid.NewGuid(), 10, "Test", "desc", Price.Create(100, null), [], "loc", Url.CreateFromSlug("slug"), _now, ChangeType.New);
        var f = notification.FormattedString();
        Assert.Contains("<b>Nuevo</b>", f);
    }

    [Fact]
    public void FormattedString_UpdatePrefix()
    {
        var notification = Notification.Create(Guid.NewGuid(), 10, "Test", "desc", Price.Create(100, null), [], "loc", Url.CreateFromSlug("slug"), _now, ChangeType.Update);
        var f = notification.FormattedString();
        Assert.Contains("<b>Actualizacion</b>", f);
    }

    [Fact]
    public void FormattedString_PriceDropPrefix()
    {
        var notification = Notification.Create(Guid.NewGuid(), 10, "Test", "desc", Price.Create(100, 150), [], "loc", Url.CreateFromSlug("slug"), _now, ChangeType.PriceDrop);
        var f = notification.FormattedString();
        Assert.Contains("<b>Bajada Precio</b>", f);
    }
}
