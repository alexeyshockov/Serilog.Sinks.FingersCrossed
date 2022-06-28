using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Moq;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed.Tests;

[SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
public class BufferSinkTests
{
    [Theory, AutoData]
    void proxies_all_flushed_logs(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        Mock<IScopeBuffer> scope, LogEvent message1, LogEvent message2)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(false);
        scope
            .Setup(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<bool>()))
            .Returns(ImmutableQueue.Create(message1, message2));

        var sut = new BufferedSink(wrappedSink.Object, () => scope.Object, trigger);

        sut.Emit(message1);

        wrappedSink.Verify(x => x.Emit(message1), Times.Once);
        wrappedSink.Verify(x => x.Emit(message2), Times.Once);
    }

    [Theory, AutoData]
    public void proxies_everything_when_not_in_scope(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        LogEvent logEvent)
    {
        var sut = new BufferedSink(wrappedSink.Object, () => null, trigger);

        sut.Emit(logEvent);

        wrappedSink.Verify(x => x.Emit(logEvent), Times.Once);
    }

    [Theory, AutoData]
    void proxies_everything_in_already_triggered_scope(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        Mock<IScopeBuffer> scope, LogEvent message)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(true);

        var sut = new BufferedSink(wrappedSink.Object, () => scope.Object, trigger);

        sut.Emit(message);

        wrappedSink.Verify(x => x.Emit(message), Times.Once);
        scope.Verify(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory, AutoData]
    void proxies_passthrough_events(Mock<ILogEventSink> wrappedSink, Mock<IScopeBuffer> scope, LogEvent message)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(false);

        var sut = new BufferedSink(wrappedSink.Object, () => scope.Object, _ => false, _ => true);

        sut.Emit(message);

        wrappedSink.Verify(x => x.Emit(message), Times.Once);
        scope.Verify(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory, AutoData]
    void buffers_passthrough_events_that_trigger_flushing(Mock<ILogEventSink> wrappedSink, Mock<IScopeBuffer> scope,
        LogEvent message)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(false);
        scope.Setup(x => x.Enqueue(message, true)).Returns(ImmutableQueue.Create(message)).Verifiable();

        var sut = new BufferedSink(wrappedSink.Object, () => scope.Object, _ => true, _ => true);

        sut.Emit(message);

        wrappedSink.Verify(x => x.Emit(message), Times.Once);
        scope.Verify();
    }
}
