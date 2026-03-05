using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wallanoti.Src.Users.Application.Login;

namespace Wallanoti.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IMediator _mediator;

    public AuthController(ILogger<AuthController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    [Route("login/{userName}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<string>> Login(string userName)
    {
        _logger.LogInformation("User {UserName} logged in at {Time}", userName, DateTime.UtcNow);

        var request = new LoginUserRequest(userName);

        var result = await _mediator.Send(request);

        if (result is null)
        {
            return NoContent();
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("verify/{userName}/{code}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<string>> Verify(string userName, string code)
    {
        var result = await _mediator.Send(new VerifyUserRequest(userName, code));

        if (result is null)
        {
            return NoContent();
        }

        return Ok(result);
    }
}