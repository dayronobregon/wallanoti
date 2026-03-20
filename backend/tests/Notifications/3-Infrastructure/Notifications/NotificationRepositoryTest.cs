using Microsoft.EntityFrameworkCore;
using Wallanoti.Src.Notifications.Infrastructure.Percistence;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework;
using Wallanoti.Src.Shared.Infrastructure.Percistence.EntityFramework.EntityModels;

namespace Wallanoti.Tests.Notifications._3_Infrastructure.Notifications;

public class NotificationRepositoryTest
{
    [Fact]
    public async Task GetLatestByUserAndUrls_ReturnsOnlyLatestSnapshotPerUrl()
    {
        await using var dbContext = BuildDbContext();
        var userId = 7L;
        var oldDate = DateTime.UtcNow.AddMinutes(-20);
        var newDate = DateTime.UtcNow.AddMinutes(-5);

        dbContext.Notifications.AddRange(
            NewEntity(userId, "https://es.wallapop.com/item/one", 100, oldDate),
            NewEntity(userId, "https://es.wallapop.com/item/one", 90, newDate),
            NewEntity(userId, "https://es.wallapop.com/item/two", 50, oldDate));
        await dbContext.SaveChangesAsync();

        var sut = new NotificationRepository(dbContext);

        var result = await sut.GetLatestByUserAndUrls(
            userId,
            new[] { "https://es.wallapop.com/item/one", "https://es.wallapop.com/item/two" },
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(90, result["https://es.wallapop.com/item/one"].LastNotifiedCurrentPrice);
        Assert.Equal(newDate, result["https://es.wallapop.com/item/one"].NotifiedAt);
        Assert.Equal(50, result["https://es.wallapop.com/item/two"].LastNotifiedCurrentPrice);
    }

    [Fact]
    public async Task GetLatestByUserAndUrls_DoesNotLeakSnapshotsAcrossUsers()
    {
        await using var dbContext = BuildDbContext();
        var requestedUser = 11L;
        var sharedUrl = "https://es.wallapop.com/item/shared";

        dbContext.Notifications.AddRange(
            NewEntity(requestedUser, sharedUrl, 120, DateTime.UtcNow.AddMinutes(-10)),
            NewEntity(99, sharedUrl, 60, DateTime.UtcNow.AddMinutes(-1)));
        await dbContext.SaveChangesAsync();

        var sut = new NotificationRepository(dbContext);

        var result = await sut.GetLatestByUserAndUrls(
            requestedUser,
            new[] { sharedUrl },
            CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(120, result[sharedUrl].LastNotifiedCurrentPrice);
    }

    private static WallanotiDbContext BuildDbContext()
    {
        var options = new DbContextOptionsBuilder<WallanotiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new WallanotiDbContext(options);
    }

    private static NotificationEntity NewEntity(long userId, string url, double currentPrice, DateTime createdAt)
    {
        return new NotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "title",
            Description = "description",
            CurrentPrice = currentPrice,
            PreviousPrice = null,
            Images = [],
            Location = "city",
            Url = url,
            CreatedAt = createdAt
        };
    }
}
