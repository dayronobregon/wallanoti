using System;

namespace Wallanoti.Src.Shared.Domain;

/// <summary>
/// Static wrapper to hold the application's current TimeProvider.
/// This allows switching the TimeProvider (for example to a Spain-specific
/// implementation) without touching all call sites that used TimeProvider.System.
/// </summary>
public static class AppTime
{
    // Default to the system TimeProvider so tests and existing behavior remain unchanged.
    public static TimeProvider Current { get; set; } = TimeProvider.System;
}
