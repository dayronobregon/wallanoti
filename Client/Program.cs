using Azure.Messaging.ServiceBus;
using Client.Extension.DependencyInjection;
using Client.HostedServices;
using Client.SkeduledTasks;
using Coravel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Telegram;
using WallapopNotification.Shared._3_Infraestrcture.Events.AzureServiceBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

//SckeduledTasks
builder.Services.AddTransient<AlertSearcher>();
builder.Services.AddTransient<ResetLastSearch>();
builder.Services.AddScheduler();


//HostedServices
builder.Services.AddHostedService<TelegramHS>();

var app = builder.Build();

//get health check
app.UseHealthChecks("/health");

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<AlertSearcher>()
        .EveryTenSeconds();

    scheduler
        .Schedule<ResetLastSearch>()
        .DailyAt(0, 0);
});

new DomainEventConsumer(app.Services.GetRequiredService<ServiceBusClient>(), app.Services).Consume();

app.Run();