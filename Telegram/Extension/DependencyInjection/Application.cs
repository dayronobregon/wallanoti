using Telegram.Bot.Configurations;
using Telegram.Bot.Handlers;
using Telegram.Bot.Handlers.MessageResolver;
using WallapopNotification.Alert._1_Domain.Events;
using WallapopNotification.Alert._2_Application.SearchAlertInWallapop;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.User._2_Application.CreateUser;
using WallapopNotification.User._2_Application.NotifyUser;
using WallapopNotification.User._3_Infraestructure.Notification;

namespace Telegram.Extension.DependencyInjection;

public static class Application
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining(typeof(CreateUserCommandHandler)); });

        services.AddScoped<DomainEventHandler<NewSearchEvent>, NotifyOnNewSearchEvent>();

        services.AddScoped<AlertSearcher>();
        services.AddScoped<TelegramBotConnection>();
        services.AddScoped<TelegramBot>();
        services.AddScoped<OnMessageHandlerFactory>();
        services.AddScoped<OnUpdateHandlerFactory>();
        services.AddScoped<StartTelegramMessageResolver>();
        services.AddScoped<NewAlertTelegramMessageResolver>();
        services.AddScoped<ListTelegramMessageResolver>();

        return services;
    }
}