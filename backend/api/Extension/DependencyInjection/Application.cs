using Wallanoti.Api.Telegram.Configurations;
using Wallanoti.Api.Telegram.Handlers;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Alerts.Application.SearchNewItems;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;
using Wallanoti.Src.Users.Application.CreateUser;

namespace Wallanoti.Api.Extension.DependencyInjection;

public static class Application
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining(typeof(CreateUserCommandHandler)); });

        services.AddScoped<ItemSearcher>();
        services.AddScoped<TelegramBotConnection>();
        services.AddScoped<ITelegramBotConnection>(provider => provider.GetRequiredService<TelegramBotConnection>());
        services.AddScoped<TelegramBot>();
        services.AddScoped<OnMessageHandlerFactory>();
        services.AddScoped<OnUpdateHandlerFactory>();
        services.AddScoped<StartTelegramMessageResolver>();
        services.AddScoped<NewAlertTelegramMessageResolver>();
        services.AddScoped<ListTelegramMessageResolver>();
        services.AddScoped<AlertUrlTelegramMessageResolver>();
        services.AddScoped<CancelTelegramMessageResolver>();
        services.AddScoped<ITelegramConversationRepository, RedisTelegramConversationRepository>();

        return services;
    }
}