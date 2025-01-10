using System.Text.Json;
using MediatR;

namespace WallapopNotification.Shared._1_Domain.Events;

public abstract class DomainEvent
{
    public abstract string EventName();
    public abstract string ToJson();
    public abstract DomainEvent FromJson(string json);
}