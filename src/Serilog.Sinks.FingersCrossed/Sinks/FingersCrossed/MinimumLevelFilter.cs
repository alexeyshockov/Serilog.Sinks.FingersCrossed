using System.Collections.Immutable;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed;

/// <seealso cref="Serilog.Core.LevelOverrideMap"/>
internal class MinimumLevelFilter
{
    private readonly record struct LevelOverride
    {
        public readonly string Context;
        public readonly string ContextPrefix;

        public readonly LogEventLevel Level;

        public LevelOverride(string source, LogEventLevel level)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentOutOfRangeException(nameof(source));

            Level = level;
            Context = source.Trim();
            ContextPrefix = Context + ".";
        }

        public bool Matches(string? context) =>
            context is not null && (context.StartsWith(ContextPrefix) || context == Context);
    }

    private readonly LogEventLevel _level;

    private readonly ImmutableArray<LevelOverride> _overrides;

    public MinimumLevelFilter(LogEventLevel level, IEnumerable<KeyValuePair<string, LogEventLevel>>? overrides = null)
    {
        _level = level;
        _overrides = (overrides ?? ImmutableArray<KeyValuePair<string, LogEventLevel>>.Empty)
            .OrderByDescending(x => x.Key)
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .Select(x => new LevelOverride(x.Key, x.Value))
            .ToImmutableArray();
    }

    public bool Match(LogEvent logEvent)
    {
        if (_overrides.IsEmpty)
            return logEvent.Level >= _level;

        logEvent.Properties.TryGetValue(Constants.SourceContextPropertyName, out var value);
        var level = value switch
        {
            ScalarValue { Value: not null } context => DetermineLevel(context.Value as string),
            _ => _level
        };

        return logEvent.Level >= level;
    }

    private LogEventLevel DetermineLevel(string? context)
    {
        foreach (var levelOverride in _overrides)
            if (levelOverride.Matches(context))
                return levelOverride.Level;

        return _level;
    }
}
