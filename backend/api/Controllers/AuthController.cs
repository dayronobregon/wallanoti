using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting("auth-login")]
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

    [HttpPost]
    [Route("verify")]
    [EnableRateLimiting("auth-verify")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<string>> Verify([FromBody] VerifyRequest request)
    {
        var result = await _mediator.Send(new VerifyUserRequest(request.UserName, request.Code));

        if (result is null)
        {
            return NoContent();
        }

        return Ok(result);
    }
}

public sealed record VerifyRequest(
    [property: Required, StringLength(64, MinimumLength = 3)] string UserName,
    [property: Required, StringLength(6, MinimumLength = 6)] string Code);
