using MediatR;
using Wallanoti.Src.Shared.Domain;
using Wallanoti.Src.Users.Domain.Exceptions;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Src.Users.Application.Details;

public record GetUserDetailsQuery : IRequest<UserDetailsResponse>;

public sealed class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, UserDetailsResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly UserContext _userContext;

    public GetUserDetailsQueryHandler(
        IUserRepository userRepository,
        UserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<UserDetailsResponse> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var user = await _userRepository.Find(userId);

        return new UserDetailsResponse(user.Id, user.UserName);
    }
}
