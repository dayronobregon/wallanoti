using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Src.Users.Infrastructure.Percistence;

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

    public Task<User> Find(long telegramUserId)
    {
        return _dbContext.Users.FirstAsync(u => u.Id == telegramUserId);
    }

    public Task<User?> Search(string userName)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public Task Save(User user)
    {
        _dbContext.Update(user);

        return _dbContext.SaveChangesAsync();
    }

    public Task<bool> Exists(User user)
    {
        return _dbContext.Users.AnyAsync(u => u.Id == user.Id || u.UserName == user.UserName);
    }
}