using MediatR;
using Microsoft.AspNetCore.Http;
using Wallanoti.Src.Shared.Domain;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Src.Users.Application.Details;

public record GetUserDetailsQuery : IRequest<User>;

public sealed class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, User>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly UserContext _userContext;

    public GetUserDetailsQueryHandler(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository,
        UserContext userContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<User> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var user = await _userRepository.Find(userId);

        //TODO eliminar VerificationCode antes de devolver el usuario. Crear un DTO
        //TODO tratar de obtener la imagen del usuario de telegram para mostrar en la web

        return user;
    }
}