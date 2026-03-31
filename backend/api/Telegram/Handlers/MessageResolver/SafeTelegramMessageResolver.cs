using Telegram.Bot.Types;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public abstract class SafeTelegramMessageResolver : IMessageResolver
{
    private const string DefaultFriendlyErrorMessage = "Ha ocurrido un error. Será notificado al administrador.";

    protected readonly IPushNotificationSender PushNotificationSender;
    private ILogger Logger { get; }

    protected SafeTelegramMessageResolver(
        IPushNotificationSender pushNotificationSender,
        ILogger logger)
    {
        PushNotificationSender = pushNotificationSender;
        Logger = logger;
    }

    public async Task Execute(Message message)
    {
        var chatId = message.Chat.Id;

        try
        {
            await ExecuteCore(message);
        }
        catch (Exception exception)
        {
            Logger.LogError(
                exception,
                "Failed to execute telegram resolver. chatId={ChatId}, resolver={Resolver}, exceptionType={ExceptionType}",
                chatId,
                ResolverName,
                exception.GetType().Name);

            await SendFriendlyErrorAsync(chatId);
        }
    }

    protected abstract Task ExecuteCore(Message message);

    protected virtual string FriendlyErrorMessage => DefaultFriendlyErrorMessage;

    protected virtual string ResolverName => GetType().Name;

    private async Task SendFriendlyErrorAsync(long chatId)
    {
        try
        {
            await PushNotificationSender.Notify(chatId, FriendlyErrorMessage);
        }
        catch (Exception exception)
        {
            Logger.LogWarning(
                exception,
                "Failed to send telegram friendly error. chatId={ChatId}, resolver={Resolver}, exceptionType={ExceptionType}",
                chatId,
                ResolverName,
                exception.GetType().Name);
        }
    }
}
