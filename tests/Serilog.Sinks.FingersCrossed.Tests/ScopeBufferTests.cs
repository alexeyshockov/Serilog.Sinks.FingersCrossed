using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed.Tests;

[SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
public class ScopeBufferTests
{
    [Theory, AutoData]
    void does_not_transit_back_when_triggered(LogEvent logEvent1, LogEvent logEvent2)
    {
        var sut = new ScopeBuffer(new LogBuffer());

        // Trigger
        sut.Enqueue(logEvent1, _ => true);

        // And check the next one that does not match
        sut.Enqueue(logEvent2, _ => false);

        sut.FlushTriggered.Should().BeTrue();
    }

    [Theory, AutoData]
    void returns_nothing_if_not_triggered_yet(LogEvent logEvent)
    {
        var sut = new ScopeBuffer(new LogBuffer());

        var buffer = sut.Enqueue(logEvent, _ => false);

        sut.FlushTriggered.Should().BeFalse();
        buffer.Should().BeEmpty();
    }

    [Theory, AutoData]
    void returns_buffered_when_triggered(LogEvent logEvent1, LogEvent logEvent2, LogEvent logEvent3)
    {
        var sut = new ScopeBuffer(new LogBuffer());

        // Buffer
        sut.Enqueue(logEvent1, _ => false);
        sut.Enqueue(logEvent2, _ => false);

        // And check when triggered
        var buffer = sut.Enqueue(logEvent3, _ => true);

        sut.FlushTriggered.Should().BeTrue();
        buffer.Should().HaveCount(3).And.ContainInOrder(logEvent1, logEvent2, logEvent3);
    }

    [Theory, AutoData]
    void does_not_buffer_if_already_triggered(Predicate<LogEvent> trigger, LogEvent logEvent1, LogEvent logEvent2)
    {
        var sut = new ScopeBuffer(new LogBuffer());

        // Trigger
        sut.Enqueue(logEvent1, _ => true);

        // And check the next one
        var buffer = sut.Enqueue(logEvent2, trigger);

        buffer.Should().HaveCount(1).And.Contain(logEvent2);
    }
}
