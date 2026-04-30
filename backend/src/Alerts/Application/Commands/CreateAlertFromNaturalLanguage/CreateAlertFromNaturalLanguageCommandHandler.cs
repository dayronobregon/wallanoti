using MediatR;
using Microsoft.Extensions.Logging;
using Wallanoti.Src.Alerts.Application.CreateAlert;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Alerts.Domain.Services;

namespace Wallanoti.Src.Alerts.Application.Commands.CreateAlertFromNaturalLanguage;

/// <summary>
/// Handler for CreateAlertFromNaturalLanguageCommand.
/// Orchestrates: NER extraction → URL building → dispatch to CreateAlertCommand flow.
/// </summary>
public sealed class CreateAlertFromNaturalLanguageCommandHandler
    : IRequestHandler<CreateAlertFromNaturalLanguageCommand, CreateAlertFromNaturalLanguageResponse>
{
    private readonly INerService _nerService;
    private readonly IWallapopUrlBuilder _urlBuilder;
    private readonly IMediator _mediator;
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<CreateAlertFromNaturalLanguageCommandHandler> _logger;

    public CreateAlertFromNaturalLanguageCommandHandler(
        INerService nerService,
        IWallapopUrlBuilder urlBuilder,
        IMediator mediator,
        IAlertRepository alertRepository,
        ILogger<CreateAlertFromNaturalLanguageCommandHandler> logger)
    {
        _nerService = nerService;
        _urlBuilder = urlBuilder;
        _mediator = mediator;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<CreateAlertFromNaturalLanguageResponse> Handle(
        CreateAlertFromNaturalLanguageCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Extract entities from natural language query using NER
        NerEntities entities;
        try
        {
            entities = await _nerService.ExtractEntitiesAsync(request.NaturalLanguageQuery, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "NER service timed out while processing query");
            throw;
        }

        // Step 2: Validate that NER extracted at least keywords
        if (entities.Keywords.Count == 0)
        {
            throw new NerExtractionFailedException(entities);
        }

        // Step 3: Log warning if optional entities are missing
        var missingEntities = new List<string>();
        if (entities.MaxPrice == null) missingEntities.Add("price");
        if (entities.Category == null) missingEntities.Add("category");
        if (entities.Location == null) missingEntities.Add("location");
        
        if (missingEntities.Count > 0)
        {
            _logger.LogWarning("Optional entities missing: {MissingEntities}", string.Join(", ", missingEntities));
        }

        // Step 4: Build Wallapop URL from extracted entities
        var wallapopUrl = _urlBuilder.BuildUrl(entities);

        // Step 5: Create alert using the existing Alert creation flow via Mediator
        var createAlertCommand = new CreateAlertCommand(
            request.UserId,
            request.NaturalLanguageQuery,
            wallapopUrl);
        
        await _mediator.Send(createAlertCommand, cancellationToken);

        // Step 6: Get the created alert details to return in response
        // The alert was just created by CreateAlertCommandHandler with the query as name
        var alerts = await _alertRepository.GetByUserId(request.UserId);
        var createdAlert = alerts.LastOrDefault(a => a.Name == request.NaturalLanguageQuery && a.Url.Value == wallapopUrl);

        if (createdAlert == null)
        {
            throw new InvalidOperationException("Failed to retrieve created alert.");
        }

        return new CreateAlertFromNaturalLanguageResponse(
            createdAlert.Id,
            createdAlert.Name,
            createdAlert.Url.Value,
            createdAlert.IsActive,
            createdAlert.CreatedAt);
    }
}
