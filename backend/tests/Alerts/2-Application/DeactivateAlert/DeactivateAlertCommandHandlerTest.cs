using Moq;
using Wallanoti.Src.Alerts.Application.DeactivateAlert;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Tests.Alerts._2_Application.DeactivateAlert;

public class DeactivateAlertCommandHandlerTest
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly DeactivateAlertCommandHandler _sut;

    public DeactivateAlertCommandHandlerTest()
    {
        _alertRepositoryMock.Setup(x => x.Update(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);
        _sut = new DeactivateAlertCommandHandler(_alertRepositoryMock.Object, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenAlertNotFound_ThrowsInvalidOperationException()
    {
        var command = new DeactivateAlertCommand(Guid.NewGuid(), 10);
        _alertRepositoryMock.Setup(x => x.SearchById(command.AlertId, command.UserId)).ReturnsAsync((Alert?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command, CancellationToken.None));

        _alertRepositoryMock.Verify(x => x.Update(It.IsAny<Alert>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeactivatesAlertAndPersistsChange()
    {
        var alert = new Alert(Guid.NewGuid(), 5, "alert", "https://es.wallapop.com/item/slug", DateTime.UtcNow,
            DateTime.UtcNow, true);

        _alertRepositoryMock.Setup(x => x.SearchById(alert.Id, alert.UserId)).ReturnsAsync(alert);

        await _sut.Handle(new DeactivateAlertCommand(alert.Id, alert.UserId), CancellationToken.None);

        Assert.False(alert.IsActive);
        _alertRepositoryMock.Verify(x => x.Update(alert), Times.Once);
    }
}
