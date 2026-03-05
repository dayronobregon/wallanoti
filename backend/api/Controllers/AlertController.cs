using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallanoti.Src.AlertCounter.Application.Queries;
using Wallanoti.Src.AlertCounter.Domain;
using Wallanoti.Src.Alerts.Application.ActivateAlert;
using Wallanoti.Src.Alerts.Application.CreateAlert;
using Wallanoti.Src.Alerts.Application.DeactivateAlert;
using Wallanoti.Src.Alerts.Application.DeleteAlert;
using Wallanoti.Src.Alerts.Application.GetByUser;
using Wallanoti.Src.Shared.Domain;

namespace Wallanoti.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public sealed class AlertController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserContext _userContext;

    public AlertController(IMediator mediator, UserContext userContext)
    {
        _mediator = mediator;
        _userContext = userContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<GetAlertsByUserIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<List<GetAlertsByUserIdResponse>> GetAlerts()
    {
        var telegramUserId = _userContext.GetUserId();
        var result = await _mediator.Send(new GetAlertsByUserIdQuery(telegramUserId));

        return result;
    }

    [HttpDelete]
    [Route("{alertId:guid}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAlert([Required] Guid alertId)
    {
        try
        {
            await _mediator.Send(new DeleteAlertCommandRequest(alertId));
        }
        catch (Exception e)
        {
            return NoContent();
        }

        return Accepted();
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAlert([Required] string alertName, [Required] string url)
    {
        var command = new CreateAlertCommand(_userContext.GetUserId(), alertName, url);

        await _mediator.Send(command);

        return Created();
    }

    [HttpPatch]
    [Route("{alertId:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateAlert([Required] Guid alertId)
    {
        try
        {
            var userId = _userContext.GetUserId();
            await _mediator.Send(new DeactivateAlertCommand(alertId, userId));

            return Accepted();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPatch]
    [Route("{alertId:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActivateAlert([Required] Guid alertId)
    {
        try
        {
            var userId = _userContext.GetUserId();
            await _mediator.Send(new ActivateAlertCommand(alertId, userId));

            return Accepted();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [Route("{alertId:guid}/counter")]
    [ProducesResponseType(typeof(AlertCounter), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAlertCounter([Required] Guid alertId)
    {
        var result = await _mediator.Send(new GetAlertCounterByAlertIdQuery(alertId));

        return Ok(result);
    }
}