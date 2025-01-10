using Client.Bot.Handlers.OnMessage;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WallapopNotification.User._3_Infraestructure.Notification;

namespace Client.Bot.Configurations;

public sealed class TelegramBot
{
    private readonly TelegramBotConnection _telegramBotConnection;
    private readonly OnMessageHandler _onMessageHandler;

    public TelegramBot(TelegramBotConnection telegramBotConnection, OnMessageHandler onMessageHandler)
    {
        _telegramBotConnection = telegramBotConnection;
        _onMessageHandler = onMessageHandler;
    }

    public async Task Start()
    {
        var botClient = _telegramBotConnection.BotClient();

        await Prepare();

        botClient.OnMessage += OnMessage;
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        await _onMessageHandler.Execute(message);
    }

    public async Task Stop()
    {
        var botClient = _telegramBotConnection.BotClient();

        await botClient.Close();
    }

    private async Task Prepare()
    {
        var botClient = _telegramBotConnection.BotClient();

        var commands = await botClient.GetMyCommands();

        if (commands.Any(x =>
                x.Command is StartTelegramCommandResolver.Command or NewAlertTelegramCommandResolver.Command))
        {
            return;
        }

        await botClient.DeleteMyCommands();

        await botClient.SetMyCommands(new List<BotCommand>
        {
            new() { Command = NewAlertTelegramCommandResolver.Command, Description = "Crea una nueva alerta" },
        });
    }
}