namespace Wallanoti.Src.Alerts.Infrastructure.Services;

/// <summary>
/// Configuration options for the NER (Named Entity Recognition) service.
/// Bound from the "NerService" section of appsettings.json.
/// </summary>
public sealed class NerServiceOptions
{
    public required string Provider { get; init; }

    public int TimeoutSeconds { get; init; } = 30;

    public required string ModelUrl { get; init; }

    public required string ApiKey { get; init; }
}
