using System.Reflection;
using Wallanoti.Api.Telegram.Handlers.MessageResolver;
using Wallanoti.Src.Notifications.Infrastructure.Telegram;

namespace Wallanoti.Tests.Telegram.Handlers.MessageResolver;

public class MessageResolversDependencyTest
{
    [Fact]
    public void TelegramResolvers_ShouldNotDependOnBotConnectionDirectly()
    {
        var resolverTypes = typeof(IMessageResolver).Assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                typeof(IMessageResolver).IsAssignableFrom(type) &&
                type.Namespace == typeof(IMessageResolver).Namespace)
            .ToArray();

        var offendingResolvers = resolverTypes
            .Where(HasTelegramBotConnectionDependency)
            .Select(type => type.Name)
            .ToArray();

        Assert.True(
            offendingResolvers.Length == 0,
            $"Resolvers with forbidden ITelegramBotConnection dependency: {string.Join(", ", offendingResolvers)}");
    }

    private static bool HasTelegramBotConnectionDependency(Type resolverType)
    {
        return resolverType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => parameter.ParameterType == typeof(ITelegramBotConnection));
    }
}
