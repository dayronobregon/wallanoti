using System.Data;

namespace WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;

public class DapperRepository
{
    public readonly IDbConnection DbConnection;

    public DapperRepository(IDbConnection dbConnection)
    {
        DbConnection = dbConnection;
    }
}