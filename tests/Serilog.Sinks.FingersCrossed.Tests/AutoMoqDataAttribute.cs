using System.Collections.Immutable;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.FingersCrossed.Tests;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(() =>
    {
        var f = new Fixture()
            .Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true,
                GenerateDelegates = true
            });

        f.Customize<LogEvent>(composer => composer.FromFactory(() => new LogEvent(
            f.Create<DateTimeOffset>(),
            f.Create<LogEventLevel>(),
            null,
            new MessageTemplate("Test", ImmutableArray<MessageTemplateToken>.Empty),
            ImmutableArray<LogEventProperty>.Empty
            )));

        return f;
    })
    {
    }
}
