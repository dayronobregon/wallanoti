using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;

namespace Wallanoti.Api.Telegram.Handlers;

public sealed class OnMessageHandlerFactory
{
    private readonly ILogger<OnMessageHandlerFactory> _logger;
    private readonly StartTelegramMessageResolver _startTelegramMessageResolver;
    private readonly NewAlertTelegramMessageResolver _newAlertTelegramMessageResolver;
    private readonly ListTelegramMessageResolver _listTelegramMessageResolver;
    private readonly AlertUrlTelegramMessageResolver _alertUrlTelegramMessageResolver;
    private readonly CancelTelegramMessageResolver _cancelTelegramMessageResolver;
    private readonly UnknownTelegramMessageResolver _unknownTelegramMessageResolver;
    private readonly ITelegramConversationRepository _conversationRepository;

    public OnMessageHandlerFactory(
        ILogger<OnMessageHandlerFactory> logger,
        StartTelegramMessageResolver startTelegramMessageResolver,
        NewAlertTelegramMessageResolver newAlertTelegramMessageResolver,
        ListTelegramMessageResolver listTelegramMessageResolver,
        AlertUrlTelegramMessageResolver alertUrlTelegramMessageResolver,
        CancelTelegramMessageResolver cancelTelegramMessageResolver,
        UnknownTelegramMessageResolver unknownTelegramMessageResolver,
        ITelegramConversationRepository conversationRepository)
    {
        _logger = logger;
        _startTelegramMessageResolver = startTelegramMessageResolver;
        _newAlertTelegramMessageResolver = newAlertTelegramMessageResolver;
        _listTelegramMessageResolver = listTelegramMessageResolver;
        _alertUrlTelegramMessageResolver = alertUrlTelegramMessageResolver;
        _cancelTelegramMessageResolver = cancelTelegramMessageResolver;
        _unknownTelegramMessageResolver = unknownTelegramMessageResolver;
        _conversationRepository = conversationRepository;
    }

    public async Task<IMessageResolver> HandleAsync(long chatId, string? messageText)
    {
        var token = messageText?.Trim();
        var isCommand = token?.StartsWith("/") == true;
        var commandToken = NormalizeCommandToken(token, isCommand);

        _logger.LogInformation(
            "Telegram message routing started. chatId={ChatId}, isCommand={IsCommand}, command={Command}",
            chatId,
            isCommand,
            commandToken ?? "none");

        if (commandToken == CancelTelegramMessageResolver.Command)
        {
            _logger.LogInformation(
                "Telegram message routing selected resolver. chatId={ChatId}, resolver={Resolver}, command={Command}",
                chatId,
                nameof(CancelTelegramMessageResolver),
                commandToken);

            return _cancelTelegramMessageResolver;
        }

        if (!isCommand)
        {
            var state = await _conversationRepository.GetStateAsync(chatId);
            var selectedResolver = state == ConversationState.AwaitingUrl
                ? (IMessageResolver)_alertUrlTelegramMessageResolver
                : _unknownTelegramMessageResolver;

            _logger.LogInformation(
                "Telegram message routing selected resolver. chatId={ChatId}, state={ConversationState}, resolver={Resolver}",
                chatId,
                state,
                selectedResolver.GetType().Name);

            return selectedResolver;
        }

        IMessageResolver commandResolver = commandToken switch
        {
            StartTelegramMessageResolver.Command => _startTelegramMessageResolver,
            NewAlertTelegramMessageResolver.Command => _newAlertTelegramMessageResolver,
            ListTelegramMessageResolver.Command => _listTelegramMessageResolver,
            _ => _unknownTelegramMessageResolver
        };

        _logger.LogInformation(
            "Telegram message routing selected resolver. chatId={ChatId}, command={Command}, resolver={Resolver}",
            chatId,
            commandToken ?? "none",
            commandResolver.GetType().Name);

        return commandResolver;
    }

    private static string? NormalizeCommandToken(string? token, bool isCommand)
    {
        if (!isCommand || token is null)
        {
            return null;
        }

        var commandToken = token.Split(' ').FirstOrDefault();

        if (string.IsNullOrWhiteSpace(commandToken))
        {
            return null;
        }

        var atIndex = commandToken.IndexOf('@');

        if (atIndex > 0)
        {
            commandToken = commandToken[..atIndex];
        }

        return commandToken.ToLowerInvariant();
    }
}
