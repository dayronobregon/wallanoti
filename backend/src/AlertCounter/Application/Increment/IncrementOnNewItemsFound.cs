using Wallanoti.Src.AlertCounter.Domain;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.AlertCounter.Application.Increment;

public sealed class IncrementOnNewItemsFound : IDomainEventHandler<NewItemsFoundEvent>
{
    private readonly IAlertCounterRepository _repository;

    public IncrementOnNewItemsFound(IAlertCounterRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(NewItemsFoundEvent @event)
    {
        //TODO fix
        var alertId = Guid.Parse(@event.EventId);
        var counter = await _repository.SearchByAlertId(alertId) ?? Domain.AlertCounter.New(Guid.NewGuid(), alertId);

        var count = @event.Items?.Count();

        if (count is > 0)
        {
            counter.Increment(count.Value);
            await _repository.Save(counter);
        }
    }
}