namespace Wallanoti.Src.Users.Domain.Exceptions;

public sealed class UserNotFoundException : Exception
{
    public UserNotFoundException(string userName) : base($"User with username '{userName}' was not found.")
    {
    }

    public UserNotFoundException(long userId) : base($"User with id '{userId}' was not found.")
    {
    }
}
