using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using Wallanoti.Src.AlertCounter.Application.Increment;
using Wallanoti.Src.Alerts.Domain;
using Wallanoti.Src.Notifications.Application.Notify.Telegram;
using Wallanoti.Src.Notifications.Application.Notify.Web;
using Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Users.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.MassTransit;

/// <summary>
/// Registers MassTransit with RabbitMQ using the topology defined in AGENTS.md:
///
///   Main exchange:        domain-events        (topic)
///   Error queues:         [queue]_error        (managed by MassTransit)
///   Delayed redelivery:   rabbitmq_delayed_message_exchange plugin required
///
/// Queue naming:  wallanoti.[module].[consumer_name_snake_case]
/// Routing keys:  [aggregate].[event_name_snake_case]
///
/// Retry policy:  2 immediate retries + delayed redelivery (5s, 15s, 30s)
/// </summary>
public static class MassTransitBusConfiguration
{
    private const string MainExchange = "domain-events";

    public static IServiceCollection AddMassTransitEventBus(
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
                // MassTransit v8 does not have a 4-arg Host() overload with port.
                // Use a URI so the port can be specified explicitly.
                var vhost = config.VirtualHost.TrimStart('/');
                var uri = new Uri($"amqp://{config.HostName}:{config.Port}/{vhost}");
                cfg.Host(uri, h =>
                {
                    h.Username(config.UserName);
                    h.Password(config.Password);
                });

                // Disable MassTransit's default publish topology for DomainEventEnvelope.
                // We control exchange routing explicitly via ReceiveEndpoint bindings.
                cfg.Publish<DomainEventEnvelope>(p => p.Exclude = true);

                ConfigureRetryPolicy(cfg);

                // ── Notifications ──────────────────────────────────────────────

                cfg.ReceiveEndpoint(
                    "wallanoti.notifications.save_notification_on_new_items_found",
                    e => ConfigureEndpoint<NewItemsFoundEvent, SaveNotificationOnNewItemsFound>(
                        e, ctx, "alert.items-found"));

                cfg.ReceiveEndpoint(
                    "wallanoti.notifications.notify_on_notification_created_push",
                    e => ConfigureEndpoint<NotificationCreatedEvent, NotifyOnNotificationCreatedPush>(
                        e, ctx, "notification.created"));

                cfg.ReceiveEndpoint(
                    "wallanoti.notifications.notify_on_notification_created_web",
                    e => ConfigureEndpoint<NotificationCreatedEvent, NotifyOnNotificationCreatedWeb>(
                        e, ctx, "notification.created"));

                cfg.ReceiveEndpoint(
                    "wallanoti.notifications.notify_on_user_logged_in_push",
                    e => ConfigureEndpoint<UserLoggedInDomainEvent, NotifyOnUserLoggedInPush>(
                        e, ctx, "user.logged-in"));

                // ── Alert Counter ───────────────────────────────────────────────

                cfg.ReceiveEndpoint(
                    "wallanoti.alert_counter.increment_on_new_items_found",
                    e => ConfigureEndpoint<NewItemsFoundEvent, IncrementOnNewItemsFound>(
                        e, ctx, "alert.items-found"));
            });
        });

        return services;
    }

    /// <summary>
    /// Configures a receive endpoint with:
    /// - Custom topology (no auto-generated exchange per message type)
    /// - Binding to the main domain-events topic exchange with the given routing key
    /// - Retry and delayed redelivery policies
    /// </summary>
    private static void ConfigureEndpoint<TEvent, THandler>(
        IRabbitMqReceiveEndpointConfigurator e,
        IBusRegistrationContext ctx,
        string routingKey)
        where TEvent : Domain.Events.DomainEvent
        where THandler : class, Domain.Events.IDomainEventHandler<TEvent>
    {
        // Prevent MassTransit from auto-creating a per-message-type exchange
        e.ConfigureConsumeTopology = false;

        // Bind this queue to the main topic exchange with the domain event routing key
        e.Bind(MainExchange, b =>
        {
            b.ExchangeType = ExchangeType.Topic;
            b.RoutingKey = routingKey;
            b.Durable = true;
            b.AutoDelete = false;
        });

        // Retry: 2 immediate attempts before redelivery kicks in
        e.UseMessageRetry(r => r.Interval(2, TimeSpan.FromSeconds(1)));

        // Delayed redelivery requires the rabbitmq_delayed_message_exchange plugin.
        // Messages exhausting redelivery are moved to [queue]_error by MassTransit.
        e.UseDelayedRedelivery(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)));

        e.ConfigureConsumer<DomainEventConsumerAdapter<TEvent, THandler>>(ctx);
    }

    private static void ConfigureRetryPolicy(IRabbitMqBusFactoryConfigurator cfg)
    {
        // No global retry here — each endpoint has its own policy above.
        // This keeps retry behaviour explicit and per-consumer.
    }
}
