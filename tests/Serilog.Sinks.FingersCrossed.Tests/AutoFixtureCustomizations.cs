using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.FingersCrossed.Tests;

internal class FromAttribute : CustomizeAttribute
{
    private readonly LogEventLevel _value;

    public FromAttribute(LogEventLevel value)
    {
        _value = value;
    }

    public override ICustomization GetCustomization(ParameterInfo parameter) => new LogLevelGenerator
    {
        From = _value
    };
}

internal class UpToAttribute : CustomizeAttribute
{
    private readonly LogEventLevel _value;

    public UpToAttribute(LogEventLevel value)
    {
        _value = value;
    }

    public override ICustomization GetCustomization(ParameterInfo parameter) => new LogLevelGenerator
    {
        UpTo = _value
    };
}

internal record LogLevelGenerator : ISpecimenBuilder, ICustomization
{
    private readonly EnumGenerator _builder = new();

    public LogEventLevel? Level { get; init; } // Exact
    public LogEventLevel? From { get; init; } // Including
    public LogEventLevel? UpTo { get; init; } // Excluding

    public void Customize(IFixture fixture) => fixture.Customize<LogEventLevel>(_ => this);

    public object Create(object request, ISpecimenContext context) => request switch
    {
        Type rt when rt == typeof(LogEventLevel) => Level ?? Sequence<LogEventLevel>(context)
            .First(level => (From is null || level >= From) && (UpTo is null || level < UpTo)),
        _ => new NoSpecimen()
    };

#pragma warning disable S2190
    [SuppressMessage("ReSharper", "IteratorNeverReturns")]
    private IEnumerable<T> Sequence<T>(ISpecimenContext context) where T : Enum
    {
        while (true)
            yield return (T)_builder.Create(typeof(T), context);
    }
#pragma warning restore S2190
}

internal record LogEventGenerator : ISpecimenBuilder, ICustomization
{
    public string? Source { get; init; }

    public void Customize(IFixture fixture) => fixture.Customizations.Add(this);

    public object Create(object request, ISpecimenContext context) => request switch
    {
        Type rt when rt == typeof(MessageTemplate) => new MessageTemplate(
            context.Create<string>(),
            ImmutableArray<MessageTemplateToken>.Empty),
        Type rt when rt == typeof(LogEvent) => new LogEvent(
            context.Create<DateTimeOffset>(),
            context.Create<LogEventLevel>(),
            null,
            context.Create<MessageTemplate>(),
            Source is not null
                ? new[] { new LogEventProperty(Constants.SourceContextPropertyName, new ScalarValue(Source)) }
                : ImmutableArray<LogEventProperty>.Empty
        ),
        _ => new NoSpecimen()
    };
}
