using Moq;
using Wallanoti.Src.Alerts.Application.GetByUser;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Tests.Alerts._2_Application.GetByUser;

public class GetAlertsByUserIdHandlerTest
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly GetAlertsByUserIdHandler _sut;

    public GetAlertsByUserIdHandlerTest()
    {
        _sut = new GetAlertsByUserIdHandler(_alertRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedAlertsForUser()
    {
        var alerts = new List<Alert>
        {
            new Alert(Guid.NewGuid(), 9, "first", "https://es.wallapop.com/item/a", DateTime.UtcNow,
                DateTime.UtcNow, true),
            new Alert(Guid.NewGuid(), 9, "second", "https://es.wallapop.com/item/b", DateTime.UtcNow,
                DateTime.UtcNow, false)
        };

        _alertRepositoryMock.Setup(x => x.GetByUserId(9)).ReturnsAsync(alerts);

        var result = await _sut.Handle(new GetAlertsByUserIdQuery(9), CancellationToken.None);

        Assert.Equal(alerts.Count, result.Count);

        var first = result[0];
        Assert.Equal(alerts[0].Id, first.Id);
        Assert.Equal(alerts[0].Name, first.Name);
        Assert.Equal(alerts[0].Url.Value, first.Url);
        Assert.Equal(alerts[0].IsActive, first.IsActive);
    }
}
