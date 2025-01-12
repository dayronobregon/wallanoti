using System.Text;

namespace WallapopNotification.User._1_Domain.Models;

public sealed class Notification
{
    public required long ToUserId { get; set; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required Price Price { get; init; }
    public List<string>? Images { get; init; }
    public required string Location { get; init; }
    public required Link Link { get; init; }

    public string FormattedString()
    {
        var message = new StringBuilder();
        message.Append($"<b>{Title}</b>\n");
        message.Append($"<i>{Description}</i>\n");
        message.Append($"<b>Price:</b> {Price.CurrentPrice}\n");
        message.Append($"<b>Location:</b> {Location}\n");
        message.Append('\n'); // Agregar una línea en blanco
        message.Append($"<a href='{Link.Url}'>{Title}</a>\n");

        return message.ToString();
    }
}
