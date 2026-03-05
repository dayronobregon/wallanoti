using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Wallanoti.Src.Shared.Domain;

namespace Wallanoti.Api.Middlewares.Auth;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserContext userContext)
    {
        var endpoint = context.GetEndpoint();
        var authorizeAttribute = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>();

        if (authorizeAttribute == null)
        {
            await _next(context);
            return;
        }
        
        var telegramUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = context.User.FindFirst(JwtRegisteredClaimNames.PreferredUsername)?.Value;

        if (string.IsNullOrEmpty(telegramUserId) || string.IsNullOrEmpty(userName) || context.User.Identity is not { IsAuthenticated: true })
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var bearerToken = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        userContext.SetUser(bearerToken, long.Parse(telegramUserId), userName);

        await _next(context);
    }
}