using Telegram.Bot.Handlers.MessageResolver;
using Telegram.Bot.Types;

namespace Telegram.Bot.Handlers;

public sealed class OnMessageHandlerFactory
{
    private readonly StartTelegramMessageResolver _startTelegramMessageResolver;
    private readonly NewAlertTelegramMessageResolver _newAlertTelegramMessageResolver;
    private readonly ListTelegramMessageResolver _listTelegramMessageResolver;

    public OnMessageHandlerFactory(StartTelegramMessageResolver startTelegramMessageResolver,
        NewAlertTelegramMessageResolver newAlertTelegramMessageResolver,
        ListTelegramMessageResolver listTelegramMessageResolver)
    {
        _startTelegramMessageResolver = startTelegramMessageResolver;
        _newAlertTelegramMessageResolver = newAlertTelegramMessageResolver;
        _listTelegramMessageResolver = listTelegramMessageResolver;
    }

    public IMessageResolver Handle(string? command)
    {
        return command switch
        {
            NewAlertTelegramMessageResolver.Command => _newAlertTelegramMessageResolver,
            ListTelegramMessageResolver.Command => _listTelegramMessageResolver,
            _ => _startTelegramMessageResolver
        };
    }
}