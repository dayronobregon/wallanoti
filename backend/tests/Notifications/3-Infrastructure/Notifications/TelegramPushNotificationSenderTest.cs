using Moq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Notifications;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._3_Infrastructure.Notifications;

public class TelegramPushNotificationSenderTest
{
    private readonly Mock<ITelegramBotClient> _botClientMock = new();
    private readonly Mock<ITelegramBotConnection> _botConnectionMock = new();

    public TelegramPushNotificationSenderTest()
    {
        _botConnectionMock.Setup(x => x.Client()).Returns(_botClientMock.Object);
    }

    [Fact]
    public async Task Notify_WhenMediaGroupGets429_RetriesAndSucceeds()
    {
        var sender = new TelegramPushNotificationSender(_botConnectionMock.Object);
        var notification = BuildNotification(new List<string> { "https://image-1", "https://image-2" });

        _botClientMock
            .SetupSequence(x => x.SendRequest(It.IsAny<SendMediaGroupRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException(
                "Too Many Requests: retry after 1",
                429,
                new ResponseParameters { RetryAfter = 1 }))
            .ReturnsAsync(new[] { new Message() });

        await sender.Notify(notification);

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMediaGroupRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Notify_WhenMediaGroupGets429Repeatedly_ThrowsAfterFiniteRetries()
    {
        var sender = new TelegramPushNotificationSender(_botConnectionMock.Object);
        var notification = BuildNotification(new List<string> { "https://image-1", "https://image-2" });

        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMediaGroupRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException(
                "Too Many Requests: retry after 1",
                429,
                new ResponseParameters { RetryAfter = 1 }));

        await Assert.ThrowsAsync<ApiRequestException>(() => sender.Notify(notification));

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMediaGroupRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(4));
    }

    [Fact]
    public async Task Notify_WhenPlainMessageGets429_RetriesAndSucceeds()
    {
        var sender = new TelegramPushNotificationSender(_botConnectionMock.Object);
        var notification = BuildNotification(Array.Empty<string>());

        _botClientMock
            .SetupSequence(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException(
                "Too Many Requests: retry after 1",
                429,
                new ResponseParameters { RetryAfter = 1 }))
            .ReturnsAsync(new Message());

        await sender.Notify(notification);

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task NotifyByUserId_WhenPlainMessageGets429Repeatedly_ThrowsAfterFiniteRetries()
    {
        var sender = new TelegramPushNotificationSender(_botConnectionMock.Object);

        _botClientMock
            .Setup(x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ApiRequestException(
                "Too Many Requests: retry after 1",
                429,
                new ResponseParameters { RetryAfter = 1 }));

        await Assert.ThrowsAsync<ApiRequestException>(() => sender.Notify(123, "test message"));

        _botClientMock.Verify(
            x => x.SendRequest(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(4));
    }

    private static Notification BuildNotification(IReadOnlyList<string> images)
    {
        return Notification.Create(
            Guid.NewGuid(),
            123,
            "title",
            "description",
            Price.Create(10, 12),
            images.ToList(),
            "city",
            Url.CreateFromSlug("slug"),
            new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        );
    }
}
