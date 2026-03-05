using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Wallanoti.Src.Alerts.Application.SearchNewItems;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Domain.ValueObjects;
using Wallanoti.Tests.Alerts._1_Domain;

namespace Wallanoti.Tests.Alerts._2_Application.SearchNewItems;

public class ItemSearcherTest : TestBase
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock = new();
    private readonly Mock<IWallapopRepository> _wallapopRepositoryMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly ItemSearcher _sut;

    public ItemSearcherTest(EventBusFixture fixture) : base(fixture)
    {
        _sut = new ItemSearcher(
            EventBus,
            _alertRepositoryMock.Object,
            _wallapopRepositoryMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task METHOD()
    {
        var alerts = new AlertFaker().Generate(2);
        var items = new ItemFaker().Generate(2);
        // Arrange
        _alertRepositoryMock.Setup(x => x.All())
            .ReturnsAsync(alerts);

        _wallapopRepositoryMock.Setup(x => x.Latest(It.IsAny<Url>()))
            .ReturnsAsync(items);

        await _sut.Execute();
    }
}