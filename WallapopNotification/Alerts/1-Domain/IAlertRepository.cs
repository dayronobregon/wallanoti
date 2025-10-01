using WallapopNotification.Alerts._1_Domain.Models;

namespace WallapopNotification.Alerts._1_Domain;

public interface IAlertRepository
{
    public Task Add(Alert alert);
    public Task<IEnumerable<Alert>> All();
    public Task Update(Alert alert);

    public Task<IEnumerable<Alert>> GetByUserId(long userId);
    
    public Task Delete(Guid alertId);
}