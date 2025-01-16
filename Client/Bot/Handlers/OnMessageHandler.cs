using Client.Bot.Handlers.MessageResolver;
using Telegram.Bot.Types;

namespace Client.Bot.Handlers;

public sealed class OnMessageHandler
{
    private readonly StartTelegramCommandResolver _startTelegramCommandResolver;
    private readonly NewAlertTelegramCommandResolver _newAlertTelegramCommandResolver;
    private readonly ListTelegramCommandResolver _listTelegramCommandResolver;

    public OnMessageHandler(StartTelegramCommandResolver startTelegramCommandResolver,
        NewAlertTelegramCommandResolver newAlertTelegramCommandResolver,
        ListTelegramCommandResolver listTelegramCommandResolver)
    {
        _startTelegramCommandResolver = startTelegramCommandResolver;
        _newAlertTelegramCommandResolver = newAlertTelegramCommandResolver;
        _listTelegramCommandResolver = listTelegramCommandResolver;
    }

    public async Task Execute(Message message)
    {
        var text = message.Text?.ToLower().Split(",").FirstOrDefault();

        switch (text)
        {
            case StartTelegramCommandResolver.Command:
                await _startTelegramCommandResolver.Execute(message);
                return;
            case NewAlertTelegramCommandResolver.Command:
                await _newAlertTelegramCommandResolver.Execute(message);
                break;
            case ListTelegramCommandResolver.Command:
                await _listTelegramCommandResolver.Execute(message);
                break;
        }
    }
}