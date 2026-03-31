using Telegram.Bot.Types;
using Wallanoti.Src.Notifications.Domain;

namespace Wallanoti.Api.Telegram.Handlers.MessageResolver;

public sealed class UnknownTelegramMessageResolver : SafeTelegramMessageResolver
{
    private readonly IPushNotificationSender _pushNotificationSender;

    public UnknownTelegramMessageResolver(
        IPushNotificationSender pushNotificationSender,
        ILogger<UnknownTelegramMessageResolver> logger)
        : base(pushNotificationSender, logger)
    {
        _pushNotificationSender = pushNotificationSender;
    }

    protected override async Task ExecuteCore(Message message)
    {
        const string helpMessage =
            "Uy, ese comando me ha pillado mirando ofertas de tostadoras... Yo solo sirvo para encontrar chollos en Wallapop 😄\n\n" +
            "Asi puedes usarme:\n" +
            "- /start para iniciar el bot\n" +
            "- /alert para crear una alerta (te pedire una URL de busqueda)\n" +
            "- /list para ver tus alertas\n" +
            "- /cancel para cancelar la operacion actual";

        await _pushNotificationSender.Notify(message.Chat.Id, helpMessage);
    }
}
