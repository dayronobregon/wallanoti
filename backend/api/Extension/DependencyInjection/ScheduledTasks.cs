using Coravel;
using Wallanoti.Api.ScheduledTasks;

namespace Wallanoti.Api.Extension.DependencyInjection;

public static class ScheduledTasks
{
    public static IServiceCollection AddSchedulerTask(this IServiceCollection services)
    {
        services.AddTransient<AlertSearcher>();
        services.AddTransient<ResetLastSearch>();
        services.AddScheduler();

        return services;
    }
}