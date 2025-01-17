using System.Data;
using Coravel.Invocable;
using Dapper;

namespace Telegram.SkeduledTasks;

public sealed class ResetLastSearch : IInvocable
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<ResetLastSearch> _logger;

    public ResetLastSearch(IDbConnection dbConnection, ILogger<ResetLastSearch> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task Invoke()
    {
        _logger.LogInformation("ResetLastSearch running at: {time}", DateTimeOffset.Now);

        await _dbConnection.ExecuteAsync("UPDATE Alerts SET LastSearch = NULL");

        _logger.LogInformation("ResetLastSearch finished at: {time}", DateTimeOffset.Now);
    }
}