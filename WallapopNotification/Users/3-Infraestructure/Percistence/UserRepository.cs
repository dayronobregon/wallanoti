using System.Data;
using Microsoft.EntityFrameworkCore;
using WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework;
using WallapopNotification.Users._1_Domain.Events;
using WallapopNotification.Users._1_Domain.Models;
using WallapopNotification.Users._1_Domain.Repositories;

namespace WallapopNotification.Users._3_Infraestructure.Percistence;

public sealed class UserRepository : IUserRepository
{
    private readonly WallanotiDbContext _dbContext;

    public UserRepository(WallanotiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task Add(User newUser)
    {
        _dbContext.Users.Add(newUser);

        return _dbContext.SaveChangesAsync();
    }

    public Task<User> Find(long userId)
    {
        return _dbContext.Users.FirstAsync(u => u.Id == userId);
    }

    public Task Save(User user)
    {
        _dbContext.Update(user);
        
        return _dbContext.SaveChangesAsync();
    }
}