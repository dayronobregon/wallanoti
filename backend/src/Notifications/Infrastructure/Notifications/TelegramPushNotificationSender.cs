using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Src.Notifications.Infrastructure.Notifications;

public class TelegramPushNotificationSender : IPushNotificationSender
{
    private const int MaxTooManyRequestsRetries = 3;
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30);

    private readonly ITelegramBotConnection _telegramConnection;

    public TelegramPushNotificationSender(ITelegramBotConnection telegramBotConnection)
    {
        _telegramConnection = telegramBotConnection;
    }

    public async Task Notify(Notification notification)
    {
        var botClient = _telegramConnection.Client();

        // Crear una lista de InputMediaPhoto para las imágenes
        var images = new List<IAlbumInputMedia>();

        if (notification.Images == null || notification.Images.Count == 0)
        {
            await SendMessageWithRetry(
                botClient,
                notification.UserId,
                notification.FormattedString(),
                ParseMode.Html,
                CancellationToken.None);
            return;
        }


        images.Add(new InputMediaPhoto(notification.Images.First())
        {
            Caption = notification.FormattedString(),
            ParseMode = ParseMode.Html
        });
        images.AddRange(notification.Images.Skip(1).Select(imageUrl => new InputMediaPhoto(imageUrl)));

        await SendMediaGroupWithRetry(botClient, notification.UserId, images, CancellationToken.None);
    }

    public Task Notify(long userId, string message)
    {
        var botClient = _telegramConnection.Client();

        return SendMessageWithRetry(
            botClient,
            userId,
            message,
            null,
            CancellationToken.None);
    }

    private static Task SendMessageWithRetry(
        ITelegramBotClient botClient,
        long userId,
        string message,
        ParseMode? parseMode,
        CancellationToken cancellationToken)
    {
        return ExecuteWithRetry(
            ct => parseMode.HasValue
                ? botClient.SendMessage(userId, message, parseMode: parseMode.Value, cancellationToken: ct)
                : botClient.SendMessage(userId, message, cancellationToken: ct),
            cancellationToken);
    }

    private static async Task SendMediaGroupWithRetry(
        ITelegramBotClient botClient,
        long userId,
        IEnumerable<IAlbumInputMedia> images,
        CancellationToken cancellationToken)
    {
        await ExecuteWithRetry(
            ct => botClient.SendMediaGroup(userId, images, cancellationToken: ct),
            cancellationToken);
    }

    private static async Task ExecuteWithRetry(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        var retryCount = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await action(cancellationToken);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (ApiRequestException exception) when (ShouldRetry(exception, retryCount))
            {
                retryCount++;
                var retryDelay = GetRetryDelay(exception);
                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }

    private static bool ShouldRetry(ApiRequestException exception, int retryCount)
    {
        return exception.ErrorCode == 429 && retryCount < MaxTooManyRequestsRetries;
    }

    private static TimeSpan GetRetryDelay(ApiRequestException exception)
    {
        var retryAfter = exception.Parameters?.RetryAfter;

        if (retryAfter is not > 0)
            return TimeSpan.Zero;

        return TimeSpan.FromSeconds(Math.Min(retryAfter.Value, MaxRetryDelay.TotalSeconds));
    }
}
