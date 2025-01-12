using System.Data;
using Dapper;
using Microsoft.Extensions.Azure;
using WallapopNotification.Alert._1_Domain.Repositories;
using WallapopNotification.Alert._3_Infraestructure.Percistence.Dapper;
using WallapopNotification.Alert._3_Infraestructure.Percistence.Wallapop.Repositories;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Shared._3_Infraestrcture.Events.AzureServiceBus;
using WallapopNotification.Shared._3_Infraestrcture.Percistence.Dapper;
using WallapopNotification.User._1_Domain;
using WallapopNotification.User._1_Domain.Repositories;
using WallapopNotification.User._3_Infraestructure.Notification;
using WallapopNotification.User._3_Infraestructure.Percistence.Dapper;

namespace Client.Extension.DependencyInjection;

public static class Infrastructure
{
    internal static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Dapper
        SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());
        SqlMapper.AddTypeHandler(new StringListTypeHandler());

        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));

        services.AddScoped<DapperContext>();
        services.AddScoped<IDbConnection>(provider =>
        {
            var context = provider.GetRequiredService<DapperContext>();
            var connection = context.DbConnection;
            connection.Open();
            return connection;
        });

        //Repositories
        services.AddScoped<IWallapopRepository, WallapopRepository>();
        services.AddScoped<INotificationSender, TelegramNotificationSender>();
        services.AddScoped<IUserRepository, MySqlUserRepository>();
        services.AddScoped<IAlertRepository, MySqlAlertRepository>();

        //EventBus
        services.AddSingleton<IEventBus, AzureEventBus>();
        services.AddSingleton<DomainEventConsumer>();

        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(configuration.GetValue<string>("ServiceBusConnection"));
        });


        return services;
    }
}