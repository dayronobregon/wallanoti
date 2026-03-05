using Telegram.Bot.Types;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public interface IMessageResolver
{
    public Task Execute(Message message);
}