using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Src.Users.Application.Login;

public record VerifyUserRequest(string UserName, string VerificationCode) : IRequest<string?>;

public sealed class VerifyQueryHandler : IRequestHandler<VerifyUserRequest, string?>
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public VerifyQueryHandler(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public async Task<string?> Handle(VerifyUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Search(request.UserName);

        if (user is null || !user.Verify(request.VerificationCode))
        {
            return null;
        }

        var token = GenerateJwtToken(user);

        return token;
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            //TODO revisar claims
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.PreferredUsername, user.UserName),
            ]),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = credentials,
            Audience = _configuration["Jwt:Audience"],
            Issuer = _configuration["Jwt:Issuer"]
        };

        var tokenHandler = new JsonWebTokenHandler();

        return tokenHandler.CreateToken(tokenDescriptor);
    }
}