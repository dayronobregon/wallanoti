using MediatR;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.User._1_Domain.Repositories;

namespace WallapopNotification.User._2_Application.CreateUser;

public record CreateUserCommand(long Id, string? UserName) : IRequest;

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
        var newUser = _1_Domain.Models.User.CreateUser(request.Id, request.UserName);

        var events = newUser.PullDomainEvents();

        await _userRepository.Add(newUser);

        await _eventBus.Publish(events);
    }
}