using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.AlertCounter.Domain;
using Wallanoti.Src.AlertCounter.Infrastructure.Percistence;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Alerts.Infrastructure.Percistence;
using Wallanoti.Src.Alerts.Infrastructure.Percistence.Wallapop;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Notifications;
using Wallanoti.Src.Notifications.Infrastructure.Percistence;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Infrastructure.Events.RabbitMq;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Users.Domain.Repositories;
using Wallanoti.Src.Users.Infrastructure.Percistence;

namespace Wallanoti.Api.Extension.DependencyInjection;

public static class Infrastructure
{
    internal static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Cache
        services.AddCache(configuration);

        //Database
        services.AddDatabase(configuration);

        //Repositories
        services.AddScoped<IWallapopRepository, WallapopRepository>();
        services.AddScoped<IPushNotificationSender, TelegramPushNotificationSender>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAlertCounterRepository, AlertCounterRepository>();
        services.AddScoped<IWebNotificationSender, WebNotificationSender>();

        //SignalR
        services.AddSignalR();

        //EventBus
        AddEventBus(services, configuration);

        return services;
    }

    private static IServiceCollection AddEventBus(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IEventBus, RabbitMqEventBus>();
        services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMq"));
        services.AddScoped<RabbitMqEventBusConfiguration>();
        services.AddScoped<RabbitDomainEventConsumer>();
        services.AddSingleton<RabbitMqConnection, RabbitMqConnection>();


        return services;
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<WallanotiDbContext>();

        services.AddDbContext<WallanotiDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                builder => builder.EnableRetryOnFailure()), ServiceLifetime.Transient);
    }

    private static void AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
    }
}