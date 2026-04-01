namespace Wallanoti.Src.Alerts.Domain.Models;

public sealed class LabeledAlertItem
{
    public Item Item { get; }
    public ItemNotificationLabel Label { get; }

    public LabeledAlertItem(Item item, ItemNotificationLabel label)
    {
        Item = item;
        Label = label;
    }
}
