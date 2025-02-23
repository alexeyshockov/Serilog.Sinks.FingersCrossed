using System.Collections.Immutable;

namespace Serilog.Sinks.FingersCrossed;

public interface IScopeBuffer : IDisposable
{
    void Flush();
}

internal sealed class ScopeBuffer : IScopeBuffer, BufferedSink.IScopeBuffer
{
    private ImmutableQueue<DelayedLogEvent> _logs = ImmutableQueue<DelayedLogEvent>.Empty;

    private readonly LogBuffer _scopeManager;
    private readonly ScopeBuffer? _previousBuffer;

    public ScopeBuffer(LogBuffer scopeManager)
    {
        _scopeManager = scopeManager;

        _previousBuffer = _scopeManager.Scope.Value;
        _scopeManager.Scope.Value = this;
    }

    public bool FlushTriggered { get; private set; }

    public void Enqueue(DelayedLogEvent logEvent, bool triggers)
    {
        ImmutableInterlocked.Enqueue(ref _logs, logEvent);

        if (!FlushTriggered && triggers)
            FlushTriggered = true;

        if (FlushTriggered)
            DoFlush();
    }

    public void Flush()
    {
        FlushTriggered = true;

        DoFlush();
    }

    private void DoFlush()
    {
        var logs = Interlocked.Exchange(ref _logs, ImmutableQueue<DelayedLogEvent>.Empty);
        if (logs.IsEmpty)
            return;

        foreach (var delayed in logs)
            delayed.Flush();
    }

    public void Dispose()
    {
        _scopeManager.Scope.Value = _previousBuffer;

        if (FlushTriggered)
            Flush();
    }
}
