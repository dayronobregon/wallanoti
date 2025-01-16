using WallapopNotification.Alert._1_Domain.Models;

namespace WallapopNotification.Alert._1_Domain.Repositories;

public interface IAlertRepository
{
    public Task Add(AlertEntity alert);
    public Task<IEnumerable<AlertEntity>> All();
    public Task Update(AlertEntity alert);

    public Task<IEnumerable<AlertEntity>> GetByUserId(long userId);
    
    public Task Delete(Guid alertId);
}