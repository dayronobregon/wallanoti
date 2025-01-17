using Telegram.Bot.Configurations;

namespace Telegram.HostedServices;

public sealed class TelegramHS : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramHS(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var telegramBot = scope.ServiceProvider.GetRequiredService<TelegramBot>();
        await telegramBot.Start();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var telegramBot = scope.ServiceProvider.GetRequiredService<TelegramBot>();
        await telegramBot.Stop();
    }
}