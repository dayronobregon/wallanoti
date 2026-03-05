using Wallanoti.Src.Users.Domain.Models;

namespace Wallanoti.Src.Users.Domain.Repositories;

public interface IUserRepository
{
    public Task Add(User newUser);
    public Task<User> Find(long telegramUserId);

    /// <summary>
    /// Busca un usuario por su nombre de usuario de telegram
    /// </summary>
    /// <param name="userName">Nombre de usuario de telegram</param>
    /// <returns></returns>
    public Task<User?> Search(string userName);

    public Task Save(User user);

    public Task<bool> Exists(User user);
}