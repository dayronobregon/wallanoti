using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;

public sealed class DapperContext
{
    public readonly IDbConnection DbConnection;

    public DapperContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MySqlConnection");
        DbConnection = new MySqlConnection(connectionString);
    }
}