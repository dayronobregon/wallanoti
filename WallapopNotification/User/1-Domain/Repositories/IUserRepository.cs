namespace WallapopNotification.User._1_Domain.Repositories;

public interface IUserRepository
{
    public Task Add(Models.User newUser);
    public Models.User Find(Guid userId);
    public Task Save(Models.User user);
}