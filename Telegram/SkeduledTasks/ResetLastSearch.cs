using System.Data;
using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;
using WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework;

namespace Telegram.SkeduledTasks;

public sealed class ResetLastSearch : IInvocable
{
    private readonly WallanotiDbContext _context;
    private readonly ILogger<ResetLastSearch> _logger;

    public ResetLastSearch(WallanotiDbContext context, ILogger<ResetLastSearch> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("ResetLastSearch running at: {time}", DateTimeOffset.Now);

        await _context.Database.ExecuteSqlRawAsync("UPDATE Alerts SET LastSearch = NULL");
        await _context.SaveChangesAsync();

        _logger.LogInformation("ResetLastSearch finished at: {time}", DateTimeOffset.Now);
    }
}