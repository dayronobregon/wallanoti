using MediatR;
using Wallanoti.Src.Alerts.Application.CreateAlert;

namespace Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;

/// <summary>
/// Request DTO for creating an alert from natural language input.
/// </summary>
public sealed record CreateAlertFromNaturalLanguageRequest(string NaturalLanguageQuery);

/// <summary>
/// Command to create an alert from a natural language query.
/// Uses NER to extract entities and build a Wallapop URL.
/// </summary>
public sealed record CreateAlertFromNaturalLanguageCommand(
    long UserId,
    string NaturalLanguageQuery
) : IRequest<CreateAlertFromNaturalLanguageResponse>;

/// <summary>
/// Response returned after successfully creating an alert from natural language.
/// </summary>
public sealed record CreateAlertFromNaturalLanguageResponse(
    Guid Id,
    string Name,
    string Url,
    bool IsActive,
    DateTime CreatedAt
);
