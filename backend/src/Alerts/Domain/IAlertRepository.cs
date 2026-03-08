using Wallanoti.Src.Alerts.Domain.Models;

namespace Wallanoti.Src.Alerts.Domain;

public interface IAlertRepository
{
    public Task Add(Alert alert);
    public Task<IEnumerable<Alert>> All();
    public Task Update(Alert alert);
    public Task<int> TouchAlert(Guid alertId, DateTime touchedAt);
    public Task<int> UpdateLastSearchedAt(Guid alertId, DateTime lastSearchedAt);

    public Task<IEnumerable<Alert>> GetByUserId(long userId);
    public Task<Alert?> SearchById(Guid alertId, long userId);

    public Task<bool> Delete(Guid alertId);
}
