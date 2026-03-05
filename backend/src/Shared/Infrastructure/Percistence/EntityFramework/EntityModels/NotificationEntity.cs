namespace Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

public sealed class NotificationEntity
{
    public required Guid Id { get; init; }
    public required long UserId { get; set; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required double CurrentPrice { get; init; }
    public double? PreviousPrice { get; init; }
    public List<string>? Images { get; init; }
    public required string Location { get; init; }
    public required string Url { get; init; }
    public required DateTime CreatedAt { get; set; }
}