namespace Wallanoti.Src.AlertCounter.Domain;

public sealed class AlertCounter
{
    public Guid Id { get; }
    public Guid AlertId { get; }
    public long Total { get; private set; }

    private AlertCounter(Guid id, Guid alertId, long total)
    {
        Id = id;
        AlertId = alertId;
        Total = total;
    }

    public static AlertCounter New(Guid id, Guid alertId)
    {
        return new AlertCounter(id, alertId, 0);
    }

    public void Increment(long count)
    {
        Total += count;
    }
}