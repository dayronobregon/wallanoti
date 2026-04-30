namespace Wallanoti.Src.Alerts.Domain.DTOs;

/// <summary>
/// Represents entities extracted from natural language text using NER.
/// </summary>
public sealed record NerEntities(
    IReadOnlyList<string> Keywords,
    string? Brand,
    decimal? MaxPrice,
    decimal? MinPrice,
    string? Category,
    string? Location
);
