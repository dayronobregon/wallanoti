using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WallapopNotification.User._1_Domain;

namespace WallapopNotification.User._3_Infraestructure.Notification;

public class TelegramNotificationSender : INotificationSender
{
    private readonly TelegramBotConnection _telegramConnection;

    public TelegramNotificationSender(TelegramBotConnection telegramBotConnection)
    {
        _telegramConnection = telegramBotConnection;
    }

    public async Task Notify(_1_Domain.Models.Notification notification)
    {
        var botClient = _telegramConnection.BotClient();

        // Crear una lista de InputMediaPhoto para las imágenes
        var images = new List<IAlbumInputMedia>();

        if (notification.Images == null || notification.Images.Count == 0)
        {
            await botClient.SendMessage(notification.ToUserId, notification.FormattedString(), ParseMode.Html);
            return;
        }


        images.Add(new InputMediaPhoto(notification.Images.First())
        {
            Caption = notification.FormattedString(),
            ParseMode = ParseMode.Html
        });
        images.AddRange(notification.Images.Skip(1).Select(imageUrl => new InputMediaPhoto(imageUrl)));

        await botClient.SendMediaGroup(notification.ToUserId, images);
    }
}