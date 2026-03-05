using Wallanoti.Src.Shared.Domain.ValueObjects;

namespace Wallanoti.Src.Alerts.Domain.Models;

public interface IWallapopRepository
{
    public Task<List<Item>?> Latest(Url url);
}