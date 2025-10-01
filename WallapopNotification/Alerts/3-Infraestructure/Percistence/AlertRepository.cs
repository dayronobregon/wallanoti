using Microsoft.EntityFrameworkCore;
using WallapopNotification.Alerts._1_Domain;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework;

namespace WallapopNotification.Alerts._3_Infraestructure.Percistence;

public sealed class AlertRepository :  IAlertRepository
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
        return await _context.Alerts.ToListAsync();
    }

    public async Task Update(Alert alert)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Alert>> GetByUserId(long userId)
    {
        return await _context.Alerts
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task Delete(Guid alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId);
        
        if (alert != null)
        {
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
}