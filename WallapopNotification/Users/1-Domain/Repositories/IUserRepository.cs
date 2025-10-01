using WallapopNotification.Users._1_Domain.Models;

namespace WallapopNotification.Users._1_Domain.Repositories;

public interface IUserRepository
{
    public Task Add(User newUser);
    public Task<User> Find(long userId);
    public Task Save(User user);

    public Task<bool> Exists(User user);
}