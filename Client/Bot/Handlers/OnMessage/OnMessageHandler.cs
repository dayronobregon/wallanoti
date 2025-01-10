using Telegram.Bot.Types;

namespace Client.Bot.Handlers.OnMessage;

public sealed class OnMessageHandler
{
    private readonly StartTelegramCommandResolver _startTelegramCommandResolver;
    private readonly NewAlertTelegramCommandResolver _newAlertTelegramCommandResolver;

    public OnMessageHandler(StartTelegramCommandResolver startTelegramCommandResolver,
        NewAlertTelegramCommandResolver newAlertTelegramCommandResolver)
    {
        _startTelegramCommandResolver = startTelegramCommandResolver;
        _newAlertTelegramCommandResolver = newAlertTelegramCommandResolver;
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
        }
    }
}