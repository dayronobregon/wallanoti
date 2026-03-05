namespace Wallanoti.Src.AlertCounter.Domain;

public interface IAlertCounterRepository
{
    public Task Save(AlertCounter counter);
    public Task<AlertCounter?> SearchByAlertId(Guid alertId);
}