using Coravel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Telegram;
using Telegram.Extension.DependencyInjection;
using Telegram.HostedServices;
using Telegram.SkeduledTasks;
using WallapopNotification.Shared._1_Domain.Events;
using WallapopNotification.Shared._3_Infrastructure.Events.RabbitMq;
using WallapopNotification.Users._2_Application.NotifyUser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSchedulerTask();

//HostedServices
builder.Services.AddHostedService<TelegramHS>();

var app = builder.Build();

app.UseHealthChecks("/health");

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<AlertSearcher>()
        .EveryFiveSeconds()
        .PreventOverlapping("AlertSearcher");

    scheduler
        .Schedule<ResetLastSearch>()
        .Monthly();
});

using (var scope = app.Services.CreateScope())
{
    var busConfiguration = scope.ServiceProvider.GetRequiredService<RabbitMqEventBusConfiguration>();
    await busConfiguration.Configure();

    var consumer = scope.ServiceProvider.GetRequiredService<RabbitDomainEventConsumer>();
    await consumer.Consume();
}


app.Run();