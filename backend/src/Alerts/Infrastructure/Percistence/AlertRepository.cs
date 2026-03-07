using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;

namespace Wallanoti.Src.Alerts.Infrastructure.Percistence;

public sealed class AlertRepository : IAlertRepository
{
    private readonly WallanotiDbContext _context;

    public AlertRepository(WallanotiDbContext context)
    {
        _context = context;
    }

    public async Task Add(Alert alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Alert>> All()
    {
        return await _context.Alerts.Where(x => x.IsActive == true).ToListAsync();
    }

    public async Task Update(Alert alert)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task TouchAlert(Guid alertId, DateTime touchedAt)
    {
        await _context.Alerts
            .Where(x => x.Id == alertId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(alert => alert.UpdatedAt, touchedAt));
    }

    public async Task<IEnumerable<Alert>> GetByUserId(long userId)
    {
        return await _context.Alerts
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<Alert?> SearchById(Guid alertId, long userId)
    {
        return await _context.Alerts.Where(x => x.Id == alertId && x.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<bool> Delete(Guid alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId);

        if (alert == null)
        {
            return false;
        }

        _context.Alerts.Remove(alert);
        await _context.SaveChangesAsync();

        return true;
    }
}
