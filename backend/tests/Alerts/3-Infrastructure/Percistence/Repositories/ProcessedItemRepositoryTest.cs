using Microsoft.EntityFrameworkCore;
using Xunit;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.Configurations;
using Wallanoti.Src.Alerts.Domain.Entities;
using Wallanoti.Src.Alerts.Domain.Repositories;
using Wallanoti.Src.Shared.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Wallanoti.Tests.Alerts._3_Infrastructure.Percistence.Repositories;

public class ProcessedItemRepositoryTest : IDisposable
{
    private readonly WallanotiDbContext _context;
    private readonly ProcessedItemRepository _sut;

    public ProcessedItemRepositoryTest()
    {
        var options = new DbContextOptionsBuilder&lt;WallanotiDbContext&gt;()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _sut = new ProcessedItemRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByAlertAndItemAsync_NotExists_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByAlertAndItemAsync(Guid.NewGuid(), "nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertAsync_CreatesNewItem()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var itemId = "item123";
        var price = Price.Create(100m, 110m);
        var processedItem = new ProcessedItem(alertId, itemId, 1700000000000L, price);

        // Act
        await _sut.UpsertAsync(processedItem);

        // Assert
        var saved = await _context.Set&lt;ProcessedItem&gt;().FirstOrDefaultAsync(p =&gt; p.ItemId == itemId);
        Assert.NotNull(saved);
        Assert.Equal(alertId, saved.AlertId);
        Assert.Equal(itemId, saved.ItemId);
        Assert.Equal(1700000000000L, saved.LastModifiedAtMs);
        Assert.Equal(100m, saved.LastPrice.CurrentPrice);
    }

    [Fact]
    public async Task UpsertAsync_UpdatesExistingItem()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var itemId = "item123";
        var oldPrice = Price.Create(100m, null);
        var processedItem1 = new ProcessedItem(alertId, itemId, 1700000000000L, oldPrice);
        await _sut.UpsertAsync(processedItem1);

        var newPrice = Price.Create(90m, 100m);
        var processedItem2 = new ProcessedItem(alertId, itemId, 1700000001000L, newPrice);

        // Act
        await _sut.UpsertAsync(processedItem2);

        // Assert
        var saved = await _context.Set&lt;ProcessedItem&gt;().FirstOrDefaultAsync(p =&gt; p.ItemId == itemId);
        Assert.NotNull(saved);
        Assert.Equal(1700000001000L, saved.LastModifiedAtMs);
        Assert.Equal(90m, saved.LastPrice.CurrentPrice);
    }
}

public class TestDbContext : WallanotiDbContext
{
    public TestDbContext(DbContextOptions&lt;WallanotiDbContext&gt; options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ProcessedItemConfiguration());
    }
}