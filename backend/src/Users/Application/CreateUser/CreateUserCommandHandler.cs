using MediatR;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Users.Domain.Models;
using Wallanoti.Src.Users.Domain.Repositories;

namespace Wallanoti.Src.Users.Application.CreateUser;

public record CreateUserCommand(long Id, string UserName) : IRequest;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;

    public CreateUserCommandHandler(IEventBus eventBus, IUserRepository userRepository)
    {
        _eventBus = eventBus;
        _userRepository = userRepository;
    }

    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var newUser = User.NewUser(request.Id, request.UserName);

        var events = newUser.PullDomainEvents();

        if (await _userRepository.Exists(newUser) == false)
        {
            await _userRepository.Add(newUser);
        }

        await _eventBus.Publish(events);
    }
}