using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed.Tests;

[SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
public class ScopeBufferTests
{
    [Theory, AutoData]
    void does_not_transit_back_when_triggered(DelayedLogEvent logEvent1, DelayedLogEvent logEvent2)
    {
        var sut = new ScopeBuffer(new LogBuffer());

        sut.FlushTriggered.Should().BeFalse();

        // Trigger
        sut.Enqueue(logEvent1, true);
        sut.FlushTriggered.Should().BeTrue();

        // And the next one that does not match
        sut.Enqueue(logEvent2, false);
        sut.FlushTriggered.Should().BeTrue(); // Should not change anything
    }

    [Theory, AutoData]
    void buffers_if_not_triggered_yet(LogEvent logEvent)
    {
        var flushed = new List<LogEvent>();
        void Sink(LogEvent e) => flushed.Add(e);

        var sut = new ScopeBuffer(new LogBuffer());

        sut.Enqueue(new DelayedLogEvent(Sink, logEvent), false);

        sut.FlushTriggered.Should().BeFalse();
        flushed.Should().BeEmpty();
    }

    [Theory, AutoData]
    void flushes_buffered_events_when_triggered(LogEvent logEvent1, LogEvent logEvent2, LogEvent logEvent3)
    {
        var flushed = new List<LogEvent>();
        void Sink(LogEvent e) => flushed.Add(e);

        var sut = new ScopeBuffer(new LogBuffer());

        // Buffer
        sut.Enqueue(new DelayedLogEvent(Sink, logEvent1), false);
        sut.Enqueue(new DelayedLogEvent(Sink, logEvent2), false);

        // And check when triggered
        sut.Enqueue(new DelayedLogEvent(Sink, logEvent3), true);

        sut.FlushTriggered.Should().BeTrue();
        flushed.Should().HaveCount(3).And.ContainInOrder(logEvent1, logEvent2, logEvent3);
    }

    [Theory, AutoData]
    void does_not_buffer_if_already_triggered(LogEvent logEvent1, LogEvent logEvent2)
    {
        var flushed = new List<LogEvent>();
        void Sink(LogEvent e) => flushed.Add(e);

        var sut = new ScopeBuffer(new LogBuffer());

        // Trigger
        sut.Enqueue(new DelayedLogEvent(Sink, logEvent1), true);

        // And check the next one
        sut.Enqueue(new DelayedLogEvent(Sink, logEvent2), false);

        flushed.Should().HaveCount(2).And.ContainInOrder(logEvent1, logEvent2);
    }
}
