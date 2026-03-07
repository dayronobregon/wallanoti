using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Src.Users.Domain.Events;

public sealed class UserLoggedInDomainEvent : DomainEvent
{
    public long TelegramUserId { get; init; }
    public VerificationCode VerificationCode { get; init; } = null!;

    public UserLoggedInDomainEvent()
    {
    }

    public UserLoggedInDomainEvent(long telegramUserId, VerificationCode verificationCode)
    {
        TelegramUserId = telegramUserId;
        VerificationCode = verificationCode;
    }

    public override string EventName() => "user.logged-in";
}
