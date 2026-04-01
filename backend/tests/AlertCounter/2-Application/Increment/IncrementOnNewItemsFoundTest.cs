using Moq;
using Wallanoti.Src.AlertCounter.Application.Increment;
using Wallanoti.Src.AlertCounter.Domain;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Tests.AlertCounter._2_Application.Increment;

public class IncrementOnNewItemsFoundTest
{
    private readonly Mock<IAlertCounterRepository> _repositoryMock = new();
    private readonly IncrementOnNewItemsFound _sut;

    public IncrementOnNewItemsFoundTest()
    {
        _repositoryMock.Setup(x => x.Save(It.IsAny<Src.AlertCounter.Domain.AlertCounter>())).Returns(Task.CompletedTask);
        _sut = new IncrementOnNewItemsFound(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCounterDoesNotExist_CreatesAndSavesWithItemCount()
    {
        var alertId = Guid.NewGuid();
        var labeledItems = new List<LabeledAlertItem>
        {
            new(BuildItem("1"), ItemNotificationLabel.New),
            new(BuildItem("2"), ItemNotificationLabel.New)
        };
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"), alertId, 10,
            labeledItems);

        _repositoryMock.Setup(x => x.SearchByAlertId(alertId)).ReturnsAsync((Src.AlertCounter.Domain.AlertCounter?)null);

        await _sut.Handle(@event);

        _repositoryMock.Verify(x => x.Save(It.Is<Src.AlertCounter.Domain.AlertCounter>(c => c.AlertId == alertId && c.Total == labeledItems.Count)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenItemsAreEmpty_DoesNotPersist()
    {
        var alertId = Guid.NewGuid();
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"), alertId, 1,
            new List<LabeledAlertItem>());

        _repositoryMock.Setup(x => x.SearchByAlertId(alertId)).ReturnsAsync((Src.AlertCounter.Domain.AlertCounter?)null);

        await _sut.Handle(@event);

        _repositoryMock.Verify(x => x.Save(It.IsAny<Src.AlertCounter.Domain.AlertCounter>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCounterExists_IncrementsExistingTotal()
    {
        var alertId = Guid.NewGuid();
        var existing = Src.AlertCounter.Domain.AlertCounter.New(Guid.NewGuid(), alertId);
        existing.Increment(1);
        var labeledItems = new List<LabeledAlertItem> { new(BuildItem("new-item"), ItemNotificationLabel.New) };
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("o"), alertId, 99,
            labeledItems);

        _repositoryMock.Setup(x => x.SearchByAlertId(alertId)).ReturnsAsync(existing);

        await _sut.Handle(@event);

        _repositoryMock.Verify(x => x.Save(It.Is<Src.AlertCounter.Domain.AlertCounter>(c => c.Total == 2)), Times.Once);
    }

    private static Item BuildItem(string id)
    {
        return new Item
        {
            Id = id,
            WallapopUserId = "user",
            Title = "title",
            Description = "desc",
            CategoryId = 1,
            Price = Price.Create(10, null),
            Images = new List<string>(),
            Location = Location.Create("city", "region"),
            Shipping = false,
            Favorited = false,
            Reserved = false,
            WebSlug = $"slug-{id}",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ModifiedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}
