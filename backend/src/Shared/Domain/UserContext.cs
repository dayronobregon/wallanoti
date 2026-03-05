namespace Wallanoti.Src.Shared.Domain;

public class UserContext
{
    private string? BearerToken { get; set; }
    private long? UserId { get; set; }
    private string? UserName { get; set; }
    private bool IsAuthenticated { get; set; }

    public void SetUser(string bearerToken, long userId, string userName)
    {
        BearerToken = bearerToken;
        UserId = userId;
        UserName = userName;
        IsAuthenticated = true;
    }

    public long GetUserId()
    {
        return !IsAuthenticated || UserId == null
            ? throw new UnauthorizedAccessException("El usuario no está autenticado")
            : UserId.Value;
    }

    public string GetUserName()
    {
        return !IsAuthenticated || UserName == null
            ? throw new UnauthorizedAccessException("El usuario no está autenticado")
            : UserName;
    }
}