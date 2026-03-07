using System.Text.Json;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class NewItemsFoundEventTest
{
    [Fact]
    public void SystemTextJsonRoundtrip_ShouldPreserveUserIdAndAlertId()
    {
        var alertId = Guid.NewGuid();
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("O"), alertId, 123, []);
        var data = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<NewItemsFoundEvent>(data);

        Assert.NotNull(deserialized);
        Assert.Equal(alertId, deserialized!.AlertId);
        Assert.Equal(123, deserialized!.UserId);
        Assert.NotNull(deserialized.Items);
        Assert.Empty(deserialized.Items);
    }
}
