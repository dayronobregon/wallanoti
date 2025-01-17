using Coravel;
using Telegram.SkeduledTasks;

namespace Telegram.Extension.DependencyInjection;

public static class SkedulerTasks
{
    public static IServiceCollection AddSchedulerTask(this IServiceCollection services)
    {
        services.AddTransient<AlertSearcher>();
        services.AddTransient<ResetLastSearch>();
        services.AddScheduler();

        return services;
    }
}