using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Caching.Hybrid;
using WallapopNotification.Alerts._1_Domain;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Alerts._3_Infraestructure.Percistence;
using WallapopNotification.Alerts._3_Infraestructure.Percistence.Wallapop;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;
using WallapopNotification.Shared._3_Infrastructure.Percistence.EntityFramework;
using WallapopNotification.Users._1_Domain;
using WallapopNotification.Users._1_Domain.Repositories;
using WallapopNotification.Users._3_Infraestructure.Notification;
using WallapopNotification.Users._3_Infraestructure.Percistence;
using Url = WallapopNotification.Shared._1_Domain.ValueObject.Url;

namespace Telegram.Extension.DependencyInjection;

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
        services.AddScoped<INotificationSender, TelegramNotificationSender>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();

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
        services.AddScoped<RabbitMqConnection>();


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