using System.Text.Json;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Domain.ValueObjects;

namespace Wallanoti.Src.Users.Domain.Events;

public sealed class UserLoggedInDomainEvent : DomainEvent
{
    public long TelegramUserId { get; }
    public VerificationCode VerificationCode { get; }

    public UserLoggedInDomainEvent()
    {
    }

    public UserLoggedInDomainEvent(long telegramUserId, VerificationCode verificationCode)
    {
        TelegramUserId = telegramUserId;
        VerificationCode = verificationCode;
    }

    public override string EventName() => "user.logged-in";

    public override string ToJson()
    {
        var obj = new
        {
            telegramUserId = TelegramUserId,
            verificationCode = VerificationCode.Value
        };

        return JsonSerializer.Serialize(obj);
    }

    public override DomainEvent FromPrimitives(string eventId, string occurredOn, string data)
    {
        var document = JsonDocument.Parse(data);
        var root = document.RootElement;

        var userId = root.GetProperty("telegramUserId").GetInt64();
        var verificationCode = root.GetProperty("verificationCode").GetString();

        return new UserLoggedInDomainEvent(userId, new VerificationCode(verificationCode));
    }
}