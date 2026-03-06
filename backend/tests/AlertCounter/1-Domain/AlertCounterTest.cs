namespace Wallanoti.Tests.AlertCounter._1_Domain;

public class AlertCounterTest
{
    [Fact]
    public void New_ShouldStartWithZeroTotal()
    {
        var alertId = Guid.NewGuid();

        var counter = Src.AlertCounter.Domain.AlertCounter.New(Guid.NewGuid(), alertId);

        Assert.Equal(0, counter.Total);
        Assert.Equal(alertId, counter.AlertId);
    }

    [Fact]
    public void Increment_ShouldIncreaseTotalByCount()
    {
        var counter = Src.AlertCounter.Domain.AlertCounter.New(Guid.NewGuid(), Guid.NewGuid());

        counter.Increment(3);

        Assert.Equal(3, counter.Total);
    }
}
