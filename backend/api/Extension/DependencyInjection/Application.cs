using Wallanoti.Api.Telegram.Configurations;
using Wallanoti.Api.Telegram.Handlers;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.AlertCounter.Application.Increment;
using Wallanoti.Src.Alerts.Application.SearchNewItems;
using Wallanoti.Src.Notifications.Application.Notify.Telegram;
using Wallanoti.Src.Notifications.Application.Notify.Web;
using Wallanoti.Src.Notifications.Application.SaveOnNewItemsFound;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;
using Wallanoti.Src.Shared.Helpers;
using Wallanoti.Src.Shared.Infrastructure.Events.RabbitMq;
using Wallanoti.Src.Users.Application.CreateUser;

namespace Wallanoti.Api.Extension.DependencyInjection;

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

        services.AddScoped<NotifyOnNotificationCreatedPush>();
        services.AddScoped<NotifyOnUserLoggedInPush>();
        services.AddScoped<NotifyOnNotificationCreatedWeb>();
        
        services.AddScoped<IncrementOnNewItemsFound>();
        services.AddScoped<SaveNotificationOnNewItemsFound>();

        services.AddDomainEventSubscriberInformationService(
            AssemblyHelper.GetInstance(Assemblies.WallapopNotification));

        return services;
    }
}