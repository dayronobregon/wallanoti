using Microsoft.EntityFrameworkCore;
using Xunit;
using Wallanoti.Src.Notifications.Domain;
using Wallanoti.Src.Notifications.Infrastructure.Persistence;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

namespace Wallanoti.Tests.Notifications._3_Infrastructure.Percistence;

public class ProcessedWallapopItemRepositoryTest
{
    [Fact]
    public async Task GetByAlertAndItemAsync_WhenNotExists_ReturnsNull()
    {
        var options = new DbContextOptionsBuilder&lt;WallanotiDbContext&gt;()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new WallanotiDbContext(options);
        context.Database.EnsureCreated();
        var repository = new ProcessedWallapopItemRepository(context);
        var result = await repository.GetByAlertAndItemAsync(Guid.NewGuid(), &quot;nonexistent&quot;);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_CreatesWhenNotExists()
    {
        var options = new DbContextOptionsBuilder&lt;WallanotiDbContext&gt;()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new WallanotiDbContext(options);
        context.Database.EnsureCreated();
        var repository = new ProcessedWallapopItemRepository(context);

        var id = Guid.NewGuid();
        var alertId = Guid.NewGuid();
        var itemId = &quot;item1&quot;;
        var processedAt = DateTime.UtcNow;
        var storedPrice = 100m;
        var lastModified = 123L;
        var item = new ProcessedWallapopItem(id, alertId, itemId, processedAt, storedPrice, lastModified);

        await repository.UpsertAsync(item);

        var saved = await repository.GetByAlertAndItemAsync(alertId, itemId);
        Assert.NotNull(saved);
        Assert.Equal(id, saved.Id);
        Assert.Equal(alertId, saved.AlertId);
        Assert.Equal(itemId, saved.WallapopItemId);
        Assert.Equal(processedAt, saved.ProcessedAtUtc, TimeSpan.FromSeconds(1)); // tolerance
        Assert.Equal(storedPrice, saved.StoredPrice);
        Assert.Equal(lastModified, saved.LastWallapopModifiedUtc);
    }

    [Fact]
    public async Task UpsertAsync_UpdatesWhenExists()
    {
        var options = new DbContextOptionsBuilder&lt;WallanotiDbContext&gt;()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        await using var context = new WallanotiDbContext(options);
        context.Database.EnsureCreated();
        var repository = new ProcessedWallapopItemRepository(context);

        var id = Guid.NewGuid();
        var alertId = Guid.NewGuid();
        var itemId = &quot;item1&quot;;
        var processedAt1 = DateTime.UtcNow.AddHours(-1);
        var storedPrice1 = 100m;
        var lastModified1 = 123L;
        var item1 = new ProcessedWallapopItem(id, alertId, itemId, processedAt1, storedPrice1, lastModified1);

        await repository.UpsertAsync(item1);

        var processedAt2 = DateTime.UtcNow;
        var storedPrice2 = 90m;
        var lastModified2 = 456L;
        var item2 = new ProcessedWallapopItem(id, alertId, itemId, processedAt2, storedPrice2, lastModified2);

        await repository.UpsertAsync(item2);

        var saved = await repository.GetByAlertAndItemAsync(alertId, itemId);
        Assert.NotNull(saved);
        Assert.Equal(processedAt2, saved.ProcessedAtUtc, TimeSpan.FromSeconds(1));
        Assert.Equal(storedPrice2, saved.StoredPrice);
        Assert.Equal(lastModified2, saved.LastWallapopModifiedUtc);
    }
}