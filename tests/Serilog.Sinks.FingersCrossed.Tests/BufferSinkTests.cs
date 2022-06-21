using System.Collections.Immutable;
using Moq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed.Tests;

public class BufferSinkTests
{
    [Theory]
    [AutoMoqData]
    internal void flushes_all_returned_logs(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        Mock<IScopeBuffer> scope, LogEvent logEvent1, LogEvent logEvent2)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(false);
        scope
            .Setup(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<Predicate<LogEvent>>()))
            .Returns(ImmutableQueue.Create(logEvent1, logEvent2));
        var sut = new BufferedSink(wrappedSink.Object, trigger, () => scope.Object);

        sut.Emit(logEvent1);

        wrappedSink.Verify(x => x.Emit(logEvent1), Times.Once);
        wrappedSink.Verify(x => x.Emit(logEvent2), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    public void proxies_events_if_not_in_scope(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        LogEvent logEvent)
    {
        var sut = new BufferedSink(wrappedSink.Object, trigger, () => null);

        sut.Emit(logEvent);

        wrappedSink.Verify(x => x.Emit(logEvent), Times.Once);
    }

    [Theory]
    [AutoMoqData]
    internal void proxies_events_if_scope_is_already_triggered(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        Mock<IScopeBuffer> scope, LogEvent logEvent)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(true);
        var sut = new BufferedSink(wrappedSink.Object, trigger, () => scope.Object);

        sut.Emit(logEvent);

        wrappedSink.Verify(x => x.Emit(logEvent), Times.Once);
        scope.Verify(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<Predicate<LogEvent>>()), Times.Never);
    }
}
