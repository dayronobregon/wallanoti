using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wallanoti.Api.Telegram.Handlers;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Api.Telegram.Configurations;

public sealed class TelegramBot : IAsyncDisposable
{
    private readonly ILogger<TelegramBot> _logger;
    private readonly TelegramBotConnection _telegramBotConnection;
    private readonly OnMessageHandlerFactory _onMessageHandlerFactory;
    private readonly OnUpdateHandlerFactory _onUpdateHandlerFactory;

    public TelegramBot(ILogger<TelegramBot> logger,
        TelegramBotConnection telegramBotConnection,
        OnMessageHandlerFactory onMessageHandlerFactory,
        OnUpdateHandlerFactory onUpdateHandlerFactory)
    {
        _logger = logger;
        _telegramBotConnection = telegramBotConnection;
        _onMessageHandlerFactory = onMessageHandlerFactory;
        _onUpdateHandlerFactory = onUpdateHandlerFactory;
    }

    public async Task Start()
    {
        var botClient = (TelegramBotClient)_telegramBotConnection.Client();

        await Prepare();

        botClient.OnError += OnError;
        botClient.OnMessage += OnMessage;
        botClient.OnUpdate += OnUpdate;

        _logger.LogInformation("Telegram bot polling handlers configured");
    }

    private async Task OnUpdate(Update update)
    {
        _logger.LogInformation(
            "Telegram update received. updateId={UpdateId}, updateType={UpdateType}",
            update.Id,
            update.Type);

        await ExecuteHandlerSafely(
            () => _onUpdateHandlerFactory.Execute(update),
            handler: nameof(OnUpdate),
            updateId: update.Id,
            updateType: update.Type);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        _logger.LogInformation(
            "Telegram message received. chatId={ChatId}, updateType={UpdateType}",
            message.Chat.Id,
            type);

        await ExecuteHandlerSafely(
            async () =>
            {
                var resolver = await _onMessageHandlerFactory.HandleAsync(message.Chat.Id, message.Text);

                _logger.LogInformation(
                    "Telegram message resolver execution started. chatId={ChatId}, updateType={UpdateType}, resolver={Resolver}",
                    message.Chat.Id,
                    type,
                    resolver.GetType().Name);

                await resolver.Execute(message);
            },
            handler: nameof(OnMessage),
            chatId: message.Chat.Id,
            updateType: type);
    }

    private async Task ExecuteHandlerSafely(
        Func<Task> action,
        string handler,
        long? chatId = null,
        int? updateId = null,
        UpdateType? updateType = null)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process telegram handler. handler={Handler}, chatId={ChatId}, updateId={UpdateId}, updateType={UpdateType}, exceptionType={ExceptionType}",
                handler,
                chatId,
                updateId,
                updateType,
                exception.GetType().Name);

            throw;
        }
    }

    private Task OnError(Exception exception, HandleErrorSource source)
    {
        _logger.LogError(
            exception,
            "Telegram polling error. source={Source}, exceptionType={ExceptionType}",
            source,
            exception.GetType().Name);

        return Task.CompletedTask;
    }

    private async Task Prepare()
    {
        var botClient = _telegramBotConnection.Client();

        var commands = await botClient.GetMyCommands();

        if (commands.Any(x =>
                x.Command is StartTelegramMessageResolver.Command
                    or NewAlertTelegramMessageResolver.Command
                    or ListTelegramMessageResolver.Command
                    or CancelTelegramMessageResolver.Command))
        {
            return;
        }

        await botClient.DeleteMyCommands();

        await botClient.SetMyCommands(new List<BotCommand>
        {
            new() { Command = NewAlertTelegramMessageResolver.Command, Description = "Crea una nueva alerta" },
            new() { Command = ListTelegramMessageResolver.Command, Description = "Lista las alertas" },
            new() { Command = CancelTelegramMessageResolver.Command, Description = "Cancela la operación en curso" },
        });
    }

    public async ValueTask DisposeAsync()
    {
        var botClient = (TelegramBotClient)_telegramBotConnection.Client();

        await botClient.Close();
    }
}
