using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using FluentAssertions;
using Serilog.Events;

namespace Serilog.Sinks.FingersCrossed.Tests;

[SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
public class MinimumLevelFilterTests
{
    [Theory, AutoData]
    void accepts_more_critical_levels([UpTo(LogEventLevel.Fatal)] LogEventLevel minLevel, IFixture fixture)
    {
        var message = fixture.Customize(new LogLevelGenerator
        {
            From = minLevel
        }).Create<LogEvent>();

        var sut = new MinimumLevelFilter(minLevel);

        sut.Match(message).Should().BeTrue();
    }

    [Theory, AutoData]
    void accepts_the_same_level(LogEventLevel minLevel, IFixture fixture)
    {
        var message = fixture.Customize(new LogLevelGenerator
        {
            Level = minLevel
        }).Create<LogEvent>();

        var sut = new MinimumLevelFilter(minLevel);

        sut.Match(message).Should().BeTrue();
    }

    [Theory, AutoData]
    void rejects_less_critical_levels([From(LogEventLevel.Debug)] LogEventLevel minLevel, IFixture fixture)
    {
        var message = fixture.Customize(new LogLevelGenerator
        {
            UpTo = minLevel
        }).Create<LogEvent>();

        var sut = new MinimumLevelFilter(minLevel);

        sut.Match(message).Should().BeFalse();
    }
}
