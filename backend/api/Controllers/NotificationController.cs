using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallanoti.Src.Notifications.Application.SearchByUserId;
using Wallanoti.Src.Shared.Domain;

namespace Wallanoti.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public sealed class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly UserContext _userContext;

    public NotificationController(IMediator mediator, UserContext userContext)
    {
        _mediator = mediator;
        _userContext = userContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), 200)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var query = new SearchNotificationByUserIdQuery(_userContext.GetUserId());

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}