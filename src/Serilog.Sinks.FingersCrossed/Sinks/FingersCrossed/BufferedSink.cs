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

    private readonly Predicate<LogEvent> _trigger;

    private readonly Func<IScopeBuffer?> _currentScope;

    internal BufferedSink(ILogEventSink wrappedSink, Predicate<LogEvent> trigger, Func<IScopeBuffer?> currentScope)
    {
        _wrappedSink = wrappedSink ?? throw new ArgumentNullException(nameof(wrappedSink));
        _trigger = trigger;
        _currentScope = currentScope;
    }

    [ExcludeFromCodeCoverage]
    public BufferedSink(ILogEventSink wrappedSink, Trigger trigger) :
        this(wrappedSink, trigger.Matches, LogBuffer.GetCurrentBuffer)
    {
    }

    private void DoEmit(LogEvent logEvent)
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

    private void Flush(IImmutableQueue<LogEvent> logs)
    {
        if (logs.IsEmpty)
            return;

        foreach (var logEvent in logs)
            DoEmit(logEvent);
    }

    public void Emit(LogEvent logEvent)
    {
        var scope = _currentScope();
        if (scope is null || scope.FlushTriggered)
            DoEmit(logEvent); // Just proxy it as the scope has been triggered already
        else
            Flush(scope.Enqueue(logEvent, _trigger));
    }

    public void Dispose()
    {
        (_wrappedSink as IDisposable)?.Dispose();
    }
}
