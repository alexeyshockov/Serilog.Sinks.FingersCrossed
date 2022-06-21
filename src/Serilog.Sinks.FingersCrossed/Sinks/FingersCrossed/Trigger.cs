using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed;

/// <summary>
/// Flushing trigger.
/// </summary>
public readonly record struct Trigger()
{
    /// <summary>
    /// Minimum level to trigger from.
    /// </summary>
    public LogEventLevel Level { get; init; } = LogEventLevel.Error;

    /// <summary>
    /// Trigger's activation predicate.
    /// </summary>
    /// <param name="logEvent">A log event</param>
    /// <returns>Does it activate the trigger or not</returns>
    public bool Matches(LogEvent logEvent) => logEvent.Level >= Level;
}
