using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace Wallanoti.Src.Notifications.Infrastructure.Telegram;

public sealed class TelegramBotConnection : ITelegramBotConnection
{
    private static TelegramBotClient? BotClient { get; set; }
    private readonly string _token;

    public TelegramBotConnection(IConfiguration configuration)
    {
        _token = configuration.GetValue<string>("TelegramBotToken");
    }

    public ITelegramBotClient Client()
    {
        var options = new TelegramBotClientOptions(_token) { RetryThreshold = 120, RetryCount = 2 };
        return BotClient ??= new TelegramBotClient(options);
    }

    public void OnMessage(TelegramBotClient.OnMessageHandler onMessage)
    {
        var botClient = GetClient();
        botClient.OnMessage += onMessage;
    }

    public void OnUpdate(TelegramBotClient.OnUpdateHandler onUpdate)
    {
        var botClient = GetClient();
        botClient.OnUpdate += onUpdate;
    }

    public Task Close()
    {
        var botClient = GetClient();
        return botClient.Close();
    }

    private TelegramBotClient GetClient()
    {
        return (TelegramBotClient)Client();
    }
}
