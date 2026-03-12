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
        string[] candidates = new[] { "Europe/Madrid", "Romance Standard Time" };

        foreach (var id in candidates)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        // Fallback to system local timezone if none matched
        return TimeZoneInfo.Local;
    }
}
