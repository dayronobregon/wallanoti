using WallapopNotification.Alert._1_Domain.Events;
using WallapopNotification.Shared._1_Domain.Events;

namespace WallapopNotification.Alert._1_Domain.Models;

public sealed class SearchModel : AggregateRoot
{
    public required Guid Id { get; init; }
    public required long UserId { get; init; }
    public IEnumerable<Item>? Items { get; private init; }
    public required DateTime SearcherOn { get; init; }
}