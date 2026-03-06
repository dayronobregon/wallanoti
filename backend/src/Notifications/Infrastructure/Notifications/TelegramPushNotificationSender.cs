using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Src.Notifications.Infrastructure.Notifications;

public class TelegramPushNotificationSender : IPushNotificationSender
{
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
            await botClient.SendMessage(notification.UserId, notification.FormattedString(), ParseMode.Html);
            return;
        }


        images.Add(new InputMediaPhoto(notification.Images.First())
        {
            Caption = notification.FormattedString(),
            ParseMode = ParseMode.Html
        });
        images.AddRange(notification.Images.Skip(1).Select(imageUrl => new InputMediaPhoto(imageUrl)));

        await botClient.SendMediaGroup(notification.UserId, images);
    }

    public Task Notify(long userId, string message)
    {
        var botClient = _telegramConnection.Client();
        return botClient.SendMessage(userId, message);
    }
}