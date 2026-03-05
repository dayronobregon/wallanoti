using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wallanoti.Api.Telegram.Handlers;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Configurations;

public sealed class TelegramBot : IAsyncDisposable
{
    private readonly TelegramBotConnection _telegramBotConnection;
    private readonly OnMessageHandlerFactory _onMessageHandlerFactory;
    private readonly OnUpdateHandlerFactory _onUpdateHandlerFactory;

    public TelegramBot(TelegramBotConnection telegramBotConnection,
        OnMessageHandlerFactory onMessageHandlerFactory,
        OnUpdateHandlerFactory onUpdateHandlerFactory)
    {
        _telegramBotConnection = telegramBotConnection;
        _onMessageHandlerFactory = onMessageHandlerFactory;
        _onUpdateHandlerFactory = onUpdateHandlerFactory;
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
        return _onUpdateHandlerFactory.Execute(update);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        var command = message.Text?.ToLower().Split(",").FirstOrDefault();

        var resolver = _onMessageHandlerFactory.Handle(command);

        await resolver.Execute(message);
    }

    private async Task Prepare()
    {
        var botClient = _telegramBotConnection.Client();

        var commands = await botClient.GetMyCommands();

        if (commands.Any(x =>
                x.Command is StartTelegramMessageResolver.Command or NewAlertTelegramMessageResolver.Command
                    or ListTelegramMessageResolver.Command))
        {
            return;
        }

        await botClient.DeleteMyCommands();

        await botClient.SetMyCommands(new List<BotCommand>
        {
            new() { Command = NewAlertTelegramMessageResolver.Command, Description = "Crea una nueva alerta" },
            new() { Command = ListTelegramMessageResolver.Command, Description = "Lista las alertas" },
        });
    }

    public async ValueTask DisposeAsync()
    {
        var botClient = _telegramBotConnection.Client();

        await botClient.Close();
    }
}