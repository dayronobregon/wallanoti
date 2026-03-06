using Telegram.Bot;

namespace Wallanoti.Src.Notifications.Infrastructure.Telegram;

public interface ITelegramBotConnection
{
    ITelegramBotClient Client();
    void OnMessage(TelegramBotClient.OnMessageHandler onMessage);
    void OnUpdate(TelegramBotClient.OnUpdateHandler onUpdate);
    Task Close();
}
