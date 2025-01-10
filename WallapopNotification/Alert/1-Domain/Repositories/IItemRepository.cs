using WallapopNotification.Alert._1_Domain.Models;

namespace WallapopNotification.Alert._1_Domain.Repositories;

public interface IItemRepository
{
    public Task Add(Item item);

    public Task<List<Item>?> Latest();
}