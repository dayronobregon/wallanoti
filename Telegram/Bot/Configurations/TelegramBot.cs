using Telegram.Bot.Handlers;
using Telegram.Bot.Handlers.MessageResolver;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WallapopNotification.User._3_Infraestructure.Notification;

namespace Telegram.Bot.Configurations;

public sealed class TelegramBot
{
    private readonly TelegramBotConnection _telegramBotConnection;
    private readonly OnMessageHandler _onMessageHandler;
    private readonly OnUpdateHandler _onUpdateHandler;

    public TelegramBot(TelegramBotConnection telegramBotConnection,
        OnMessageHandler onMessageHandler,
        OnUpdateHandler onUpdateHandler)
    {
        _telegramBotConnection = telegramBotConnection;
        _onMessageHandler = onMessageHandler;
        _onUpdateHandler = onUpdateHandler;
    }

    public async Task Start()
    {
        var botClient = _telegramBotConnection.Client();

        await Prepare();

        botClient.OnMessage += OnMessage;
        botClient.OnUpdate += OnUpdate;
    }

    private Task OnUpdate(Update update)
    {
        return _onUpdateHandler.Execute(update);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        await _onMessageHandler.Execute(message);
    }

    public async Task Stop()
    {
        var botClient = _telegramBotConnection.Client();

        await botClient.Close();
    }

    private async Task Prepare()
    {
        var botClient = _telegramBotConnection.Client();

        var commands = await botClient.GetMyCommands();

        if (commands.Any(x =>
                x.Command is StartTelegramCommandResolver.Command or NewAlertTelegramCommandResolver.Command
                    or ListTelegramCommandResolver.Command))
        {
            return;
        }

        await botClient.DeleteMyCommands();

        await botClient.SetMyCommands(new List<BotCommand>
        {
            new() { Command = NewAlertTelegramCommandResolver.Command, Description = "Crea una nueva alerta" },
            new() { Command = ListTelegramCommandResolver.Command, Description = "Lista las alertas" },
        });
    }
}