using System.Data;
using Dapper;
using WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;
using WallapopNotification.User._1_Domain.Repositories;

namespace WallapopNotification.User._3_Infraestructure.Percistence.Dapper;

public sealed class MySqlUserRepository : DapperRepository, IUserRepository
{
    public MySqlUserRepository(IDbConnection dbConnection) : base(dbConnection)
    {
    }

    public async Task Add(_1_Domain.Models.User newUser)
    {
        const string query = "INSERT INTO Users (Id, UserName) VALUES (@Id, @UserName);";

        await DbConnection.ExecuteAsync(query, newUser);
    }

    public _1_Domain.Models.User Find(Guid userId)
    {
        const string query = "SELECT * FROM Users WHERE Id = @Id;";

        return DbConnection.QueryFirst<_1_Domain.Models.User>(query, new { Id = userId });
    }

    public Task Save(_1_Domain.Models.User user)
    {
        const string query = "UPDATE Users SET UserName = @UserName, WHERE Id = @Id;";

        return DbConnection.ExecuteAsync(query, new { user.Id, user.UserName });
    }
}