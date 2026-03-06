using Telegram.Bot;

namespace Wallanoti.Src.Notifications.Infrastructure.Telegram;

public interface ITelegramBotConnection
{
    ITelegramBotClient Client();
}
