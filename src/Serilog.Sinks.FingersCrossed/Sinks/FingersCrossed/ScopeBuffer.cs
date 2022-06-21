using System;
using System.Collections.Immutable;
using System.Threading;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed;

internal interface IScopeBuffer
{
    bool FlushTriggered { get; }

    IImmutableQueue<LogEvent> Enqueue(LogEvent logEvent, Predicate<LogEvent> trigger);
}

internal sealed class ScopeBuffer : IScopeBuffer, IDisposable
{
    private ImmutableQueue<LogEvent> _logs = ImmutableQueue<LogEvent>.Empty;

    private readonly LogBuffer _scopeManager;
    private readonly ScopeBuffer? _previousBuffer;

    public ScopeBuffer(LogBuffer scopeManager)
    {
        _scopeManager = scopeManager;

        _previousBuffer = _scopeManager.Scope.Value;
        _scopeManager.Scope.Value = this;
    }

    public bool FlushTriggered { get; private set; }

    public IImmutableQueue<LogEvent> Enqueue(LogEvent logEvent, Predicate<LogEvent> trigger)
    {
        ImmutableInterlocked.Enqueue(ref _logs, logEvent);

        if (!FlushTriggered && trigger(logEvent))
            FlushTriggered = true;

        return FlushTriggered
            ? Interlocked.Exchange(ref _logs, ImmutableQueue<LogEvent>.Empty)
            : ImmutableQueue<LogEvent>.Empty;
    }

    public void Dispose()
    {
        _scopeManager.Scope.Value = _previousBuffer;
    }
}
