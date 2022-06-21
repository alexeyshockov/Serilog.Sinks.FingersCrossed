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
    /// <param name="triggerLevel">Minimum level which triggers the whole buffer to flush.</param>
    /// <returns>A <see cref="LoggerConfiguration"/> allowing configuration to continue.</returns>
    public static LoggerConfiguration FingersCrossed(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<LoggerSinkConfiguration> configure,
        LogEventLevel triggerLevel = LogEventLevel.Error)
    {
        return LoggerSinkConfiguration.Wrap(
            loggerSinkConfiguration,
            wrappedSink => new BufferedSink(wrappedSink, new Trigger
            {
                Level = triggerLevel
            }),
            configure,
            LevelAlias.Minimum,
            null);
    }
}
