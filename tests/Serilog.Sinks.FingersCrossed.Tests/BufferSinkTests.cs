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
    void passes_through(Mock<ILogEventSink> wrappedSink, Func<IScopeBuffer?> scope, Predicate<LogEvent> trigger,
        LogEvent message)
    {
        var sut = new BufferedSink(wrappedSink.Object, scope, trigger, _ => true);

        sut.Emit(message);

        wrappedSink.Verify(x => x.Emit(message), Times.Once);
    }

    [Theory, AutoData]
    void flushes_all_returned_logs(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        Mock<IScopeBuffer> scope, LogEvent message1, LogEvent message2)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(false);
        scope
            .Setup(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<Predicate<LogEvent>>()))
            .Returns(ImmutableQueue.Create(message1, message2));
        var sut = new BufferedSink(wrappedSink.Object, () => scope.Object, trigger);

        sut.Emit(message1);

        wrappedSink.Verify(x => x.Emit(message1), Times.Once);
        wrappedSink.Verify(x => x.Emit(message2), Times.Once);
    }

    [Theory, AutoData]
    public void proxies_events_if_not_in_scope(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        LogEvent logEvent)
    {
        var sut = new BufferedSink(wrappedSink.Object, () => null, trigger);

        sut.Emit(logEvent);

        wrappedSink.Verify(x => x.Emit(logEvent), Times.Once);
    }

    [Theory, AutoData]
    void proxies_events_if_scope_is_already_triggered(Mock<ILogEventSink> wrappedSink, Predicate<LogEvent> trigger,
        Mock<IScopeBuffer> scope, LogEvent message)
    {
        scope.SetupGet(x => x.FlushTriggered).Returns(true);
        var sut = new BufferedSink(wrappedSink.Object, () => scope.Object, trigger);

        sut.Emit(message);

        wrappedSink.Verify(x => x.Emit(message), Times.Once);
        scope.Verify(x => x.Enqueue(It.IsAny<LogEvent>(), It.IsAny<Predicate<LogEvent>>()), Times.Never);
    }
}
