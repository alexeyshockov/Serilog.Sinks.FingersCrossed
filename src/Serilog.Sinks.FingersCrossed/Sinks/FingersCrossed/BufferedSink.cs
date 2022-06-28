using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed;

internal sealed class BufferedSink : ILogEventSink, IDisposable
{
    private readonly ILogEventSink _wrappedSink;

    private readonly Func<IScopeBuffer?> _currentScope;

    private readonly Predicate<LogEvent> _flushTrigger;
    private readonly Predicate<LogEvent> _passThroughFilter;

    public BufferedSink(ILogEventSink wrappedSink, Func<IScopeBuffer?> currentScope,
        Predicate<LogEvent> flushTrigger, Predicate<LogEvent>? passThroughFilter = null)
    {
        _wrappedSink = wrappedSink;
        _currentScope = currentScope;
        _flushTrigger = flushTrigger;
        _passThroughFilter = passThroughFilter ?? (_ => false);
    }

    public void Emit(LogEvent logEvent)
    {
        var scope = _currentScope();
        if (scope is null || scope.FlushTriggered)
            Proxy(logEvent); // Just proxy it as the scope has been triggered already
        else
            Buffer(scope, logEvent);
    }

    [ExcludeFromCodeCoverage]
    private void Proxy(LogEvent logEvent)
    {
        try
        {
            _wrappedSink.Emit(logEvent);
        }
        catch (Exception e)
        {
            SelfLog.WriteLine("{0} failed to emit event to wrapped sink: {1}", typeof(BufferedSink), e);
        }
    }

    private void Buffer(IScopeBuffer scope, LogEvent logEvent)
    {
        var triggers = _flushTrigger(logEvent);
        var passesThrough = _passThroughFilter(logEvent);

        if (passesThrough && !triggers)
            Proxy(logEvent);
        else
            Flush(scope.Enqueue(logEvent, triggers));
    }

    private void Flush(IImmutableQueue<LogEvent> logs)
    {
        if (logs.IsEmpty)
            return;

        foreach (var logEvent in logs)
            Proxy(logEvent);
    }

    public void Dispose()
    {
        (_wrappedSink as IDisposable)?.Dispose();
    }
}
