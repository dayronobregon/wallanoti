using Moq;
using Wallanoti.Src.Alerts.Application.CreateAlert;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Tests.Alerts._2_Application.CreateAlert;

public class CreateAlertCommandHandlerTest
{
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly AlertCommandHanlder _sut;

    public CreateAlertCommandHandlerTest()
    {
        _eventBusMock.Setup(x => x.Publish(It.IsAny<List<DomainEvent>>()))
            .Returns(Task.CompletedTask);
        _sut = new AlertCommandHanlder(_eventBusMock.Object, _alertRepositoryMock.Object, _timeProvider);
    }

    [Fact]
    public async Task Handle_ShouldAddAlertAndPublishDomainEvents()
    {
        var command = new CreateAlertCommand(1, "alert name", "https://es.wallapop.com/item/slug");

        Alert? added = null;
        _alertRepositoryMock.Setup(x => x.Add(It.IsAny<Alert>()))
            .Callback<Alert>(alert => added = alert)
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _alertRepositoryMock.Verify(x => x.Add(It.IsAny<Alert>()), Times.Once);
        _eventBusMock.Verify(x => x.Publish(It.IsAny<List<DomainEvent>>()), Times.Once);

        Assert.NotNull(added);
        Assert.Equal(command.UserId, added!.UserId);
        Assert.Equal(command.AlertName, added.Name);
        Assert.Equal(command.AlertUrl, added.Url.Value);
    }
}
