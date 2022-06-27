using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.FingersCrossed;

namespace Serilog;

/// <summary>
/// Extends <see cref="LoggerConfiguration"/> with methods for configuring buffered logging.
/// </summary>
public static class LoggerConfigurationAsyncExtensions
{
    /// <summary>
    /// Wraps a sink into a buffered sink, with a flush trigger.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The <see cref="LoggerSinkConfiguration"/> being configured.</param>
    /// <param name="configure">An action that configures the wrapped sink.</param>
    /// <param name="flushFrom">Minimum level which triggers current scope buffer to flush.</param>
    /// <param name="configurePassThrough">A function that configures the pass through filter.</param>
    /// <returns>A <see cref="LoggerConfiguration"/> allowing configuration to continue.</returns>
    public static LoggerConfiguration FingersCrossed(this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<LoggerSinkConfiguration> configure,
        LogEventLevel flushFrom = LogEventLevel.Error,
        Func<FilterConfiguration, FilterConfiguration>? configurePassThrough = null) => FingersCrossed(loggerSinkConfiguration,
        configure, fc => fc with { MinimumLevel = flushFrom }, configurePassThrough);

    /// <summary>
    /// Wraps a sink into a buffered sink, with a flush trigger.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The <see cref="LoggerSinkConfiguration"/> being configured.</param>
    /// <param name="configure">An action that configures the wrapped sink.</param>
    /// <param name="configureFlush">A function that configures the flush trigger.</param>
    /// <param name="configurePassThrough">A function that configures the pass through filter.</param>
    /// <returns>A <see cref="LoggerConfiguration"/> allowing configuration to continue.</returns>
    public static LoggerConfiguration FingersCrossed(this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<LoggerSinkConfiguration> configure,
        Func<FilterConfiguration, FilterConfiguration> configureFlush,
        Func<FilterConfiguration, FilterConfiguration>? configurePassThrough = null) => FingersCrossed(
        loggerSinkConfiguration, configure,
        configureFlush(new FilterConfiguration()).Create(),
        configurePassThrough?.Invoke(new FilterConfiguration()).Create()
    );

    /// <summary>
    /// Wraps a sink into a buffered sink, with a flush trigger.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The <see cref="LoggerSinkConfiguration"/> being configured.</param>
    /// <param name="configure">An action that configures the wrapped sink.</param>
    /// <param name="flushTrigger">A flush trigger delegate.</param>
    /// <param name="passThroughFilter">A pass through filter delegate.</param>
    /// <returns>A <see cref="LoggerConfiguration"/> allowing configuration to continue.</returns>
    public static LoggerConfiguration FingersCrossed(this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<LoggerSinkConfiguration> configure,
        Predicate<LogEvent> flushTrigger, Predicate<LogEvent>? passThroughFilter = null)
    {
        return LoggerSinkConfiguration.Wrap(
            loggerSinkConfiguration,
            wrappedSink => new BufferedSink(wrappedSink, LogBuffer.GetCurrentBuffer,
                flushTrigger, passThroughFilter),
            configure,
            LevelAlias.Minimum,
            null);
    }
}
