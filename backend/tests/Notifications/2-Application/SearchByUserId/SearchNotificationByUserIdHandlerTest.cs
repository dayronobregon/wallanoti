using Moq;
using Wallanoti.Src.Notifications.Application.SearchByUserId;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.Notifications._2_Application.SearchByUserId;

public class SearchNotificationByUserIdHandlerTest
{
    private readonly Mock<INotificationRepository> _repositoryMock = new();
    private readonly SearchNotificationByUserIdHandler _sut;
    private readonly DateTime _now = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    public SearchNotificationByUserIdHandlerTest()
    {
        _sut = new SearchNotificationByUserIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedNotifications()
    {
        var notification = Notification.Create(Guid.NewGuid(), 1, "title", "description", Price.Create(30, 40),
            new List<string> { "img" }, "City", Url.CreateFromSlug("slug"), _now
        );

        _repositoryMock.Setup(x => x.ByUserId(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification> { notification });

        var result = await _sut.Handle(new SearchNotificationByUserIdQuery(1), CancellationToken.None);

        Assert.NotNull(result);
        var single = Assert.Single(result!);
        Assert.Equal(notification.Id, single.Id);
        Assert.Equal(notification.Title, single.Title);
        Assert.Equal(notification.Description, single.Description);
        Assert.Equal(notification.Location, single.Location);
        Assert.Equal(notification.Price.CurrentPrice, single.CurrentPrice);
        Assert.Equal(notification.Price.PreviousPrice, single.PreviousPrice);
        Assert.Equal(notification.Url.Value, single.Url);
        Assert.Equal(notification.Images, single.Images);
    }
}
