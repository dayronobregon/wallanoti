using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallanoti.Src.Users.Application.Details;

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
    public async Task<UserDetailsResponse> Index()
    {
        var result = await _mediator.Send(new GetUserDetailsQuery());

        return result;
    }
}
