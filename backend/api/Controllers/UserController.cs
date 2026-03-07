using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallanoti.Src.Users.Application.Details;
using Wallanoti.Src.Users.Domain.Exceptions;

namespace Wallanoti.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public sealed class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsResponse>> Index()
    {
        try
        {
            var result = await _mediator.Send(new GetUserDetailsQuery());

            return Ok(result);
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }
}
