using Wallanoti.Api.Telegram.Conversation;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;

namespace Wallanoti.Api.Telegram.Handlers;

public sealed class OnMessageHandlerFactory
{
    private readonly StartTelegramMessageResolver _startTelegramMessageResolver;
    private readonly NewAlertTelegramMessageResolver _newAlertTelegramMessageResolver;
    private readonly ListTelegramMessageResolver _listTelegramMessageResolver;
    private readonly AlertUrlTelegramMessageResolver _alertUrlTelegramMessageResolver;
    private readonly CancelTelegramMessageResolver _cancelTelegramMessageResolver;
    private readonly UnknownTelegramMessageResolver _unknownTelegramMessageResolver;
    private readonly ITelegramConversationRepository _conversationRepository;

    public OnMessageHandlerFactory(
        StartTelegramMessageResolver startTelegramMessageResolver,
        NewAlertTelegramMessageResolver newAlertTelegramMessageResolver,
        ListTelegramMessageResolver listTelegramMessageResolver,
        AlertUrlTelegramMessageResolver alertUrlTelegramMessageResolver,
        CancelTelegramMessageResolver cancelTelegramMessageResolver,
        UnknownTelegramMessageResolver unknownTelegramMessageResolver,
        ITelegramConversationRepository conversationRepository)
    {
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
        // Determine the command token: first word if starts with "/", else null
        var token = messageText?.Trim();
        var isCommand = token?.StartsWith("/") == true;
        string? commandToken = null;
        if (isCommand && token is not null)
        {
            // Take the first whitespace-delimited token (e.g. "/alert@MyBot")
            commandToken = token.Split(' ').FirstOrDefault();

            if (!string.IsNullOrEmpty(commandToken))
            {
                // Normalize Telegram group-chat commands like "/command@BotName" to "/command"
                var atIndex = commandToken.IndexOf('@');
                if (atIndex > 0)
                {
                    commandToken = commandToken.Substring(0, atIndex);
                }

                commandToken = commandToken.ToLower();
            }
        }
        // /cancel is always handled first regardless of state
        if (commandToken == CancelTelegramMessageResolver.Command)
            return _cancelTelegramMessageResolver;

        // If not a command, check conversation state for free-text routing
        if (!isCommand)
        {
            var state = await _conversationRepository.GetStateAsync(chatId);
            if (state == ConversationState.AwaitingUrl)
                return _alertUrlTelegramMessageResolver;

            return _unknownTelegramMessageResolver;
        }

        // Command routing
        return commandToken switch
        {
            StartTelegramMessageResolver.Command => _startTelegramMessageResolver,
            NewAlertTelegramMessageResolver.Command => _newAlertTelegramMessageResolver,
            ListTelegramMessageResolver.Command => _listTelegramMessageResolver,
            _ => _unknownTelegramMessageResolver
        };
    }
}
