using System;

namespace Wallanoti.Src.Shared.Domain;

public class SpainTimeProvider : TimeProvider
{
    private readonly TimeZoneInfo _spainZone;

    public SpainTimeProvider()
    {
        _spainZone = ResolveSpainTimeZone();
    }

    public override DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;

    public override TimeZoneInfo LocalTimeZone => _spainZone;

    // The base TimeProvider.GetLocalNow uses GetUtcNow() and LocalTimeZone.
    // No need to override it; keep default behavior to avoid signature issues.

    private static TimeZoneInfo ResolveSpainTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        }
        catch (InvalidTimeZoneException)
        {
        }
        catch (TimeZoneNotFoundException)
        {
        }

        return TimeZoneInfo.Utc;
    }
}