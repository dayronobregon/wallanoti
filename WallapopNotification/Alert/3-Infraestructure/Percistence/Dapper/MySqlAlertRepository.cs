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

    public Task Add(AlertEntity alertEntity)
    {
        const string query =
            "INSERT INTO Alerts (Id, UserId, Name, Url, CreatedAt) VALUES (@Id, @UserId, @Name, @Url, @CreatedAt);";

        return DbConnection.ExecuteAsync(query, alertEntity);
    }

    public async Task<IEnumerable<AlertEntity>> All()
    {
        const string query = "SELECT * FROM Alerts;";

        var result = await DbConnection.QueryAsync<AlertEntity>(query);

        return result;
    }

    public async Task Update(AlertEntity alert)
    {
        const string query = "UPDATE Alerts SET LastSearch = @LastSearch, SearchOn = @SearchOn WHERE Id = @Id;";

        await DbConnection.ExecuteAsync(query, new
            {
                LastSearch = JsonSerializer.Serialize(alert.LastSearch),
                Id = alert.Id,
                SearchOn = alert.SearchOn
            }
        );
    }
}