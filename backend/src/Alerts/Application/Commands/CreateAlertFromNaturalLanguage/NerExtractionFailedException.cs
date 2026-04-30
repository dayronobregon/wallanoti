using Wallanoti.Src.Alerts.Domain.DTOs;

namespace Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;

/// <summary>
/// Exception thrown when NER could not extract any keywords from the input.
/// Carries the extracted entities (empty) so the controller can include them in the response.
/// </summary>
public sealed class NerExtractionFailedException : Exception
{
    public NerEntities ExtractedEntities { get; }

    public NerExtractionFailedException(NerEntities extractedEntities)
        : base("NER could not extract any keywords from the input. Please try a different search query.")
    {
        ExtractedEntities = extractedEntities;
    }
}