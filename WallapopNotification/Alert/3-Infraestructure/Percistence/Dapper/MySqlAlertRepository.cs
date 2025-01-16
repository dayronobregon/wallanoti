using System.Data;
using System.Text.Json;
using Dapper;
using WallapopNotification.Alert._1_Domain;
using WallapopNotification.Alert._1_Domain.Models;
using WallapopNotification.Alert._1_Domain.Repositories;
using WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;
using WallapopNotification.User._1_Domain.Repositories;

namespace WallapopNotification.Alert._3_Infraestructure.Percistence.Dapper;

public sealed class MySqlAlertRepository : DapperRepository, IAlertRepository
{
    public MySqlAlertRepository(IDbConnection dbConnection) : base(dbConnection)
    {
    }

    public Task Add(AlertEntity alert)
    {
        const string query =
            "INSERT INTO alerts (Id, UserId, Name, Url, CreatedAt) VALUES (@Id, @UserId, @Name, @Url, @CreatedAt);";

        return DbConnection.ExecuteAsync(query, alert);
    }

    public async Task<IEnumerable<AlertEntity>> All()
    {
        const string query = "SELECT * FROM alerts;";

        var result = await DbConnection.QueryAsync<AlertEntity>(query);

        return result;
    }

    public async Task Update(AlertEntity alert)
    {
        const string query = "UPDATE alerts SET LastSearch = @LastSearch, SearchOn = @SearchOn WHERE Id = @Id;";

        await DbConnection.ExecuteAsync(query, new
            {
                LastSearch = JsonSerializer.Serialize(alert.LastSearch),
                Id = alert.Id,
                SearchOn = alert.SearchOn
            }
        );
    }

    public Task<IEnumerable<AlertEntity>> GetByUserId(long userId)
    {
        const string query = "SELECT * FROM alerts WHERE UserId = @UserId;";

        return DbConnection.QueryAsync<AlertEntity>(query, new { UserId = userId });
    }

    public Task Delete(Guid alertId)
    {
        const string query = "DELETE FROM alerts WHERE Id = @Id;";

        return DbConnection.ExecuteAsync(query, new { Id = alertId });
    }
}