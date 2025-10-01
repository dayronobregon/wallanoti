using WallapopNotification.Shared._1_Domain.ValueObject;

namespace WallapopNotification.Alerts._1_Domain.Models;

public interface IWallapopRepository
{
    public Task<List<Item>?> Latest(Url url);
}