using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using Wallanoti.Src.AlertCounter.Application.Increment;
using Wallanoti.Src.AlertCounter.Domain;
using Wallanoti.Src.AlertCounter.Infrastructure.Percistence;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Alerts.Domain.Models;
using Wallanoti.Src.Alerts.Domain.Services;
using Wallanoti.Src.Alerts.Infrastructure.Percistence;
using Wallanoti.Src.Alerts.Infrastructure.Percistence.Wallapop;
using Wallanoti.Src.Alerts.Infrastructure.Services;
using Wallanoti.Src.Notifications.Application.Notify.Telegram;
using Wallanoti.Src.Notifications.Application.Notify.Web;
using Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Notifications;
using Wallanoti.Src.Notifications.Infrastructure.Percistence;
using Wallanoti.Src.Shared.Domain.Events;
using Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Users.Domain.Events;
using Wallanoti.Src.Users.Domain.Repositories;
using Wallanoti.Src.Users.Infrastructure.Percistence;

namespace Wallanoti.Api.Extension.DependencyInjection;

public static class Infrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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

        //NER Service
        services.AddNerServices(configuration);

        //EventBus
        AddEventBus(services, configuration);

        return services;
    }

    private static IServiceCollection AddNerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind NerService options from configuration
        var nerOptions = configuration.GetSection("NerService").Get<NerServiceOptions>()
            ?? throw new InvalidOperationException("NerService configuration section is missing.");
        services.AddSingleton(nerOptions);

        // Register HttpClient with Polly retry policy for NER service
        services.AddHttpClient("NerService", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(nerOptions.TimeoutSeconds);
        })
        .AddPolicyHandler(GetRetryPolicy());

        // NER Service
        services.AddScoped<INerService, HuggingFaceNerService>();
        services.AddScoped<IWallapopUrlBuilder, WallapopUrlBuilder>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IServiceCollection AddEventBus(this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection("RabbitMq").Get<RabbitMqConfig>()
            ?? throw new InvalidOperationException("RabbitMq configuration section is missing.");

        // IEventBus → MassTransitEventBus (uses MassTransit IPublishEndpoint internally)
        services.AddScoped<IEventBus, MassTransitEventBus>();

        // Register MassTransit with RabbitMQ transport (default conventions)
        services.AddMassTransitEventBus(rabbitMqConfig);

        // Register domain event handlers so DI can resolve them inside the consumer adapter
        services.AddScoped<Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound.SaveNotificationOnNewItemsFound>();
        services.AddScoped<Wallanoti.Src.AlertCounter.Application.Increment.IncrementOnNewItemsFound>();
        services.AddScoped<Wallanoti.Src.Notifications.Application.Notify.Telegram.NotifyOnNotificationCreatedPush>();
        services.AddScoped<Wallanoti.Src.Notifications.Application.Notify.Web.NotifyOnNotificationCreatedWeb>();
        services.AddScoped<Wallanoti.Src.Notifications.Application.Notify.Telegram.NotifyOnUserLoggedInPush>();

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

    private static IServiceCollection AddMassTransitEventBus(
        this IServiceCollection services,
        RabbitMqConfig config)
    {
        services.AddMassTransit(x =>
        {
            // Register all consumer adapters
            x.AddConsumer<DomainEventConsumerAdapter<NewItemsFoundEvent, SaveNotificationOnNewItemsFound>>();
            x.AddConsumer<DomainEventConsumerAdapter<NewItemsFoundEvent, IncrementOnNewItemsFound>>();
            x.AddConsumer<DomainEventConsumerAdapter<NotificationCreatedEvent, NotifyOnNotificationCreatedPush>>();
            x.AddConsumer<DomainEventConsumerAdapter<NotificationCreatedEvent, NotifyOnNotificationCreatedWeb>>();
            x.AddConsumer<DomainEventConsumerAdapter<UserLoggedInDomainEvent, NotifyOnUserLoggedInPush>>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var vhost = config.VirtualHost.TrimStart('/');
                var uri = new Uri($"amqp://{config.HostName}:{config.Port}/{vhost}");
                cfg.Host(uri, h =>
                {
                    h.Username(config.UserName);
                    h.Password(config.Password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
