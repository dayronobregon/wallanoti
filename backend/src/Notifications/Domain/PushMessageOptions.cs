namespace Wallanoti.Src.Notifications.Domain;

public sealed record PushMessageOptions(bool ProtectContent, IReadOnlyList<PushActionButton> ActionButtons)
{
    public static readonly PushMessageOptions None = new(false, []);
}

public sealed record PushActionButton(string Text, string CallbackData);
