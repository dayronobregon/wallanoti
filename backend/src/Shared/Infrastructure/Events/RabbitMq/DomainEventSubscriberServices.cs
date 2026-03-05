using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wallanoti.Src.Shared.Domain.Events;

namespace Wallanoti.Src.Shared.Infrastructure.Events.RabbitMq;

public static class DomainEventSubscriberInformationService
{
    public static IServiceCollection AddDomainEventSubscriberInformationService(this IServiceCollection services,
        Assembly assembly)
    {
        var information =
            new Dictionary<Type, DomainEventSubscriberInformation>();

        var classTypes = assembly.ExportedTypes.Select(t => t.GetTypeInfo()).Where(t => t.IsClass && !t.IsAbstract);

        foreach (var type in classTypes)
        {
            var interfaces = type.ImplementedInterfaces.Select(i => i.GetTypeInfo());

            foreach (var handlerInterfaceType in interfaces.Where(i =>
                         i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
            {
                services.AddScoped(handlerInterfaceType.AsType(), type.AsType());
                FormatSubscribers(assembly, handlerInterfaceType, information);
            }
        }

        services.AddScoped(s => new DomainEventSubscribersInformation(information));
        return services;
    }

    private static void FormatSubscribers(Assembly assembly, TypeInfo handlerInterfaceType,
        Dictionary<Type, DomainEventSubscriberInformation> information)
    {
        var handlerClassTypes = assembly.GetLoadableTypes()
            .Where(handlerInterfaceType.IsAssignableFrom);

        var eventType = handlerInterfaceType.GenericTypeArguments.FirstOrDefault();

        if (eventType == null) return;

        foreach (var handlerClassType in handlerClassTypes)
        {
            if (information.ContainsKey(handlerClassType))
            {
                continue;
            }
            information.Add(handlerClassType,
                new DomainEventSubscriberInformation(handlerClassType, eventType));
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null);
        }
    }
}