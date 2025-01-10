using WallapopNotification.Alert._1_Domain.Models;

namespace WallapopNotification.Alert._1_Domain.Repositories;

public interface IWallapopRepository
{
    public Task<List<Item>?> Latest(string url);
}