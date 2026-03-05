using Coravel.Invocable;
using Wallanoti.Src.Alerts.Application.SearchNewItems;

namespace Wallanoti.Api.ScheduledTasks;

public sealed class AlertSearcher : IInvocable
{
    private readonly ILogger<AlertSearcher> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AlertSearcher(ILogger<AlertSearcher> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke()
    {
        using var scope = _serviceProvider.CreateScope();
        var alertSearcher = scope.ServiceProvider
            .GetRequiredService<ItemSearcher>();

        _logger.LogInformation("AlertSearcher start at: {time}", DateTimeOffset.Now);

        await alertSearcher.Execute();

        _logger.LogInformation("AlertSearcher finished at: {time}", DateTimeOffset.Now);
    }
}