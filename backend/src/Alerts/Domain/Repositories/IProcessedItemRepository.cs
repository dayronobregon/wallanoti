using System.Threading.Tasks;
using Wallanoti.Src.Alerts.Domain.Entities;
using Wallanoti.Src.Shared.Domain.Aggregates;

namespace Wallanoti.Src.Alerts.Domain.Repositories;

public interface IProcessedItemRepository
{
    Task<ProcessedItem?> GetByAlertAndItemAsync(Guid alertId, string itemId);
    Task UpsertAsync(ProcessedItem processedItem);
}