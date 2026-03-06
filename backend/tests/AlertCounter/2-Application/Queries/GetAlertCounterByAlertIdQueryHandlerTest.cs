using Moq;
using Wallanoti.Src.AlertCounter.Application.Queries;
using Wallanoti.Src.AlertCounter.Domain;

namespace Wallanoti.Tests.AlertCounter._2_Application.Queries;

public class GetAlertCounterByAlertIdQueryHandlerTest
{
    private readonly Mock<IAlertCounterRepository> _repositoryMock = new();
    private readonly GetAlertCounterByAlertIdQueryHandler _sut;

    public GetAlertCounterByAlertIdQueryHandlerTest()
    {
        _sut = new GetAlertCounterByAlertIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsCounterFromRepository()
    {
        var alertId = Guid.NewGuid();
        var counter = Src.AlertCounter.Domain.AlertCounter.New(Guid.NewGuid(), alertId);
        counter.Increment(4);

        _repositoryMock.Setup(x => x.SearchByAlertId(alertId)).ReturnsAsync(counter);

        var result = await _sut.Handle(new GetAlertCounterByAlertIdQuery(alertId), CancellationToken.None);

        Assert.Equal(counter, result);
    }
}
