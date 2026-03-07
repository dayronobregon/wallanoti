using System.Text.Json;
using Wallanoti.Src.Alerts.Domain;

namespace Wallanoti.Tests.Alerts._1_Domain;

public class NewItemsFoundEventTest
{
    [Fact]
    public void SystemTextJsonRoundtrip_ShouldPreserveUserId()
    {
        var @event = new NewItemsFoundEvent(Guid.NewGuid().ToString(), DateTime.UtcNow.ToString("O"), 123, []);
        var data = JsonSerializer.Serialize(@event);
        var deserialized = JsonSerializer.Deserialize<NewItemsFoundEvent>(data);

        Assert.NotNull(deserialized);
        Assert.Equal(123, deserialized!.UserId);
        Assert.NotNull(deserialized.Items);
        Assert.Empty(deserialized.Items);
    }
}
