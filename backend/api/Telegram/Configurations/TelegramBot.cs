using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wallanoti.Api.Telegram.Handlers;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Configurations;

public sealed class TelegramBot : IAsyncDisposable
{
    private readonly ITelegramBotConnection _telegramBotConnection;
    private readonly OnMessageHandlerFactory _onMessageHandlerFactory;
    private readonly OnUpdateHandlerFactory _onUpdateHandlerFactory;

    public TelegramBot(ITelegramBotConnection telegramBotConnection,
        OnMessageHandlerFactory onMessageHandlerFactory,
        OnUpdateHandlerFactory onUpdateHandlerFactory)
    {
        _telegramBotConnection = telegramBotConnection;
        _onMessageHandlerFactory = onMessageHandlerFactory;
        _onUpdateHandlerFactory = onUpdateHandlerFactory;
    }

    public async Task Start()
    {
        await Prepare();

        _telegramBotConnection.OnMessage(OnMessage);
        _telegramBotConnection.OnUpdate(OnUpdate);
    }

    private Task OnUpdate(Update update)
    {
        return _onUpdateHandlerFactory.Execute(update);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        var resolver = await _onMessageHandlerFactory.HandleAsync(message.Chat.Id, message.Text);

        await resolver.Execute(message);
    }

    private async Task Prepare()
    {
        var botClient = _telegramBotConnection.Client();

        var commands = await botClient.GetMyCommands();
        var requiredCommands = new[]
        {
            StartTelegramMessageResolver.Command,
            NewAlertTelegramMessageResolver.Command,
            ListTelegramMessageResolver.Command,
            CancelTelegramMessageResolver.Command
        };

        if (requiredCommands.All(requiredCommand =>
                commands.Any(command =>
                    command.Command.Equals(requiredCommand, StringComparison.OrdinalIgnoreCase))))
        {
            return;
        }

        await botClient.DeleteMyCommands();

        await botClient.SetMyCommands(new List<BotCommand>
        {
            new() { Command = StartTelegramMessageResolver.Command, Description = "Inicia el bot y crea tu usuario" },
            new() { Command = NewAlertTelegramMessageResolver.Command, Description = "Crea una nueva alerta" },
            new() { Command = ListTelegramMessageResolver.Command, Description = "Lista las alertas" },
            new() { Command = CancelTelegramMessageResolver.Command, Description = "Cancela la operación en curso" },
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _telegramBotConnection.Close();
    }
}
