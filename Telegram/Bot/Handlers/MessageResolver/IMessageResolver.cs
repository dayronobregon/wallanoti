using Telegram.Bot.Types;

namespace Telegram.Bot.Handlers.MessageResolver;

public interface IMessageResolver
{
    public Task Execute(Message message);
}