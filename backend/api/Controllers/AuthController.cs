using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login requested at {Time}", DateTime.UtcNow);

        var loginRequest = new LoginUserRequest(request.UserName);

        var result = await _mediator.Send(loginRequest);

        if (result is null)
        {
            return NoContent();
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("verify")]
    [EnableRateLimiting("auth-verify")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<string>> Verify([FromBody, Required] VerifyRequest request)
    {
        var result = await _mediator.Send(new VerifyUserRequest(request.UserName, request.VerificationCode));

        if (result is null)
        {
            return NoContent();
        }

        return Ok(result);
    }
}

public sealed record LoginRequest(
    [property: Required, StringLength(64, MinimumLength = 3)] string UserName);

public sealed record VerifyRequest(
    [property: Required, StringLength(64, MinimumLength = 3)] string UserName,
    [property: Required, StringLength(6, MinimumLength = 6), RegularExpression("^\\d{6}$")]
    string VerificationCode);
