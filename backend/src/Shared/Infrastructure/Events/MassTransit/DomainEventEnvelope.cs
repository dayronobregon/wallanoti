namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// Message contract used by MassTransit to transport domain events over RabbitMQ.
/// Wraps the serialized domain event payload so that MassTransit routes it through
/// a single topic exchange using the domain event name as routing key.
/// </summary>
public sealed record DomainEventEnvelope
{
    /// <summary>Unique identifier of the domain event.</summary>
    public required string EventId { get; init; }

    /// <summary>ISO-8601 timestamp when the domain event occurred.</summary>
    public required string OccurredOn { get; init; }

    /// <summary>
    /// Fully qualified type name of the concrete DomainEvent subclass.
    /// Used by the consumer adapter to locate the correct FromPrimitives factory.
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>JSON payload produced by DomainEvent.ToJson().</summary>
    public required string Data { get; init; }
}
