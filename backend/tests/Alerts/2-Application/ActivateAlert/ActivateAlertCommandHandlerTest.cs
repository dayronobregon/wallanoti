using Moq;
using Wallanoti.Src.Alerts.Application.ActivateAlert;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Tests.Alerts._2_Application.ActivateAlert;

public class ActivateAlertCommandHandlerTest
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    private readonly ActivateAlertCommandHandler _sut;

    public ActivateAlertCommandHandlerTest()
    {
        _alertRepositoryMock.Setup(x => x.Update(It.IsAny<Alert>()))
            .Returns(Task.CompletedTask);
        _sut = new ActivateAlertCommandHandler(_alertRepositoryMock.Object, _timeProvider);
    }

    [Fact]
    public async Task Handle_WhenAlertNotFound_ThrowsInvalidOperationException()
    {
        var command = new ActivateAlertCommand(Guid.NewGuid(), 1);
        _alertRepositoryMock.Setup(x => x.SearchById(command.AlertId, command.UserId)).ReturnsAsync((Alert?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Handle(command, CancellationToken.None));

        _alertRepositoryMock.Verify(x => x.Update(It.IsAny<Alert>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActivatesRequestedAlertAndDeactivatesOthers()
    {
        var userId = 1L;
        var target = new Alert(Guid.NewGuid(), userId, "target", "https://es.wallapop.com/item/a", DateTime.UtcNow,
            DateTime.UtcNow, false);
        var otherActive = new Alert(Guid.NewGuid(), userId, "other-active", "https://es.wallapop.com/item/b",
            DateTime.UtcNow, DateTime.UtcNow, true);
        var otherInactive = new Alert(Guid.NewGuid(), userId, "other-inactive", "https://es.wallapop.com/item/c",
            DateTime.UtcNow, DateTime.UtcNow, false);

        _alertRepositoryMock.Setup(x => x.SearchById(target.Id, userId)).ReturnsAsync(target);
        _alertRepositoryMock.Setup(x => x.GetByUserId(userId))
            .ReturnsAsync(new List<Alert> { target, otherActive, otherInactive });

        await _sut.Handle(new ActivateAlertCommand(target.Id, userId), CancellationToken.None);

        Assert.True(target.IsActive);
        Assert.False(otherActive.IsActive);
        Assert.False(otherInactive.IsActive);

        _alertRepositoryMock.Verify(x => x.Update(target), Times.Once);
        _alertRepositoryMock.Verify(x => x.Update(otherActive), Times.Once);
        _alertRepositoryMock.Verify(x => x.Update(otherInactive), Times.Never);
    }
}
