using System.Reflection;
using Telegram.Bot.Configurations;
using Telegram.Bot.Handlers;
using Telegram.Bot.Handlers.MessageResolver;
using WallapopNotification.Alerts._1_Domain;
using WallapopNotification.Alerts._1_Domain.Models;
using WallapopNotification.Alerts._2_Application.SearchNewItems;
using WallapopNotification.Shared;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Shared._3_Infrastructure.Events;
using WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;
using WallapopNotification.Users._2_Application.CreateUser;
using WallapopNotification.Users._2_Application.NotifyUser;
using WallapopNotification.Users._3_Infraestructure.Notification;

namespace Telegram.Extension.DependencyInjection;

public static class Application
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining(typeof(CreateUserCommandHandler)); });

        services.AddScoped<ItemSearcher>();
        services.AddScoped<TelegramBotConnection>();
        services.AddScoped<TelegramBot>();
        services.AddScoped<OnMessageHandlerFactory>();
        services.AddScoped<OnUpdateHandlerFactory>();
        services.AddScoped<StartTelegramMessageResolver>();
        services.AddScoped<NewAlertTelegramMessageResolver>();
        services.AddScoped<ListTelegramMessageResolver>();

        services.AddDomainEventSubscriberInformationService();

        return services;
    }
}