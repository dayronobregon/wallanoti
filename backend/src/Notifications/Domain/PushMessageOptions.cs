using System.Collections.Immutable;

namespace Wallanoti.Src.Notifications.Domain;

public sealed record PushMessageOptions
{
    public bool ProtectContent { get; }
    public ImmutableArray<PushActionButton> ActionButtons { get; }

    public PushMessageOptions(bool ProtectContent, IEnumerable<PushActionButton>? ActionButtons)
    {
        this.ProtectContent = ProtectContent;
        this.ActionButtons = ActionButtons?.ToImmutableArray() ?? ImmutableArray<PushActionButton>.Empty;
    }

    public static readonly PushMessageOptions None = new(false, ImmutableArray<PushActionButton>.Empty);
}

public sealed record PushActionButton(string Text, string CallbackData);
