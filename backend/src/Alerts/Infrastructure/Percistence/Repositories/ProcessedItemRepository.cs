using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Alerts.Domain.Entities;
using Wallanoti.Src.Alerts.Domain.Repositories;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;

namespace Wallanoti.Src.Alerts.Infrastructure.Percistence.Repositories;

public class ProcessedItemRepository : IProcessedItemRepository
{
    private readonly WallanotiDbContext _context;

    public ProcessedItemRepository(WallanotiDbContext context)
    {
        _context = context;
    }

    public async Task&lt;ProcessedItem?&gt; GetByAlertAndItemAsync(Guid alertId, string itemId)
    {
        return await _context.Set&lt;ProcessedItem&gt;()
            .FirstOrDefaultAsync(p =&gt; p.AlertId == alertId &amp;&amp; p.ItemId == itemId);
    }

    public async Task UpsertAsync(ProcessedItem processedItem)
    {
        var existing = await _context.Set&lt;ProcessedItem&gt;()
            .FirstOrDefaultAsync(p =&gt; p.AlertId == processedItem.AlertId &amp;&amp; p.ItemId == processedItem.ItemId);

        if (existing == null)
        {
            _context.Set&lt;ProcessedItem&gt;().Add(processedItem);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(processedItem);
            existing.RowVersion = processedItem.RowVersion;
        }

        await _context.SaveChangesAsync();
    }
}