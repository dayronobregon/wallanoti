using MediatR;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Src.Users.Application.Login;

public record LoginUserRequest(string UserName) : IRequest<string?>;

public sealed class LoginQueryHandler : IRequestHandler<LoginUserRequest, string?>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;

    public LoginQueryHandler(IUserRepository userRepository, IEventBus eventBus)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
    }

    public async Task<string?> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Search(request.UserName);

        if (user is null)
        {
            return null;
        }

        user.Login();

        await _userRepository.Save(user);

        var events = user.PullDomainEvents();

        await _eventBus.Publish(events);

        return user.UserName;
    }
}