using System;
using System.Collections.Immutable;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed;

public sealed record FilterConfiguration
{
    /// <summary>
    /// Minimum log level to accept from.
    /// </summary>
    public LogEventLevel MinimumLevel { get; init; } = LevelAlias.Minimum;

    /// <summary>
    /// Minimum log level overrides (source → level).
    /// </summary>
    public IImmutableDictionary<string, LogEventLevel> Overrides { get; init; } =
        ImmutableDictionary<string, LogEventLevel>.Empty;

    /// <summary>
    /// Overriders minimum log level for a source.
    /// </summary>
    /// <param name="source">Source context.</param>
    /// <param name="minimumLevel">Minimum log level to accept from.</param>
    /// <returns></returns>
    public FilterConfiguration Override(string source, LogEventLevel minimumLevel) =>
        this with { Overrides = Overrides.Add(source, minimumLevel) };

    // Multiple functions can be combined in the future
    internal Predicate<LogEvent> Create() => new MinimumLevelFilter(MinimumLevel, Overrides).Match;
}
