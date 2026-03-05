using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.AlertCounter.Domain;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;

namespace Wallanoti.Src.AlertCounter.Infrastructure.Percistence;

public sealed class AlertCounterRepository : IAlertCounterRepository
{
    private readonly WallanotiDbContext _context;

    public AlertCounterRepository(WallanotiDbContext context)
    {
        _context = context;
    }

    public async Task Save(Domain.AlertCounter counter)
    {
        if (_context.Entry(counter).State == EntityState.Detached)
            await _context.AddAsync(counter);
        else
            _context.Entry(counter).State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }

    public Task<Domain.AlertCounter?> SearchByAlertId(Guid alertId)
    {
        return _context.AlertCounters.FirstOrDefaultAsync(x => x.AlertId == alertId);
    }
}