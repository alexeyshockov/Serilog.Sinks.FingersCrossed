using System.Diagnostics.CodeAnalysis;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace Serilog.Sinks.FingersCrossed.Tests;

[SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
public class IntegrationTests
{
    [Fact]
    public void Basic()
    {
        using var logger = new LoggerConfiguration()
            .WriteTo.FingersCrossed(c => c.InMemory())
            .CreateLogger();

        logger.Information("Test message");

        InMemorySink.Instance.Should().HaveMessage("Test message").Appearing().Once();
    }

    [Fact]
    public void DroppedScope()
    {
        using var logger = new LoggerConfiguration()
            .WriteTo.FingersCrossed(c => c.InMemory())
            .CreateLogger();

        using (LogBuffer.BeginScope())
        {
            logger.Information("Test message");
        }

        InMemorySink.Instance.Should().NotHaveMessage();
    }

    [Fact]
    public void TriggeredScope()
    {
        using var logger = new LoggerConfiguration()
            .WriteTo.FingersCrossed(c => c.InMemory())
            .CreateLogger();

        using (LogBuffer.BeginScope())
        {
            logger.Information("Test message");
            logger.Error("Something is broken!");
        }

        InMemorySink.Instance.Should().HaveMessage("Test message").Appearing().Once();
        InMemorySink.Instance.Should().HaveMessage("Something is broken!").Appearing().Once();
    }

    [Fact]
    public async Task MultipleAsyncScopes()
    {
        var random = new Random();
        var logger = new LoggerConfiguration()
            .WriteTo.FingersCrossed(c => c.InMemory())
            .CreateLogger();

        async Task Logic(int a, int b)
        {
            logger.Information($"Checking numbers {a} and {b}...");
            await Task.Delay(random.Next(10, 1000));
            if (a != b)
                logger.Error($"{a} is not equal to {b}!");
        }

        var numbers = new[]
        {
            (1, 1),
            (2, 94),
            (3, 3),
            (4, 47)
        };

        await Task.WhenAll(numbers.Select(async x =>
        {
            using (LogBuffer.BeginScope())
                await Logic(x.Item1, x.Item2);
        }));

        InMemorySink.Instance.Should().NotHaveMessage("Checking numbers 1 and 1...");

        InMemorySink.Instance.Should().HaveMessage("Checking numbers 2 and 94...");
        InMemorySink.Instance.Should().HaveMessage("2 is not equal to 94!");

        InMemorySink.Instance.Should().NotHaveMessage("Checking numbers 3 and 3...");

        InMemorySink.Instance.Should().HaveMessage("Checking numbers 4 and 47...");
        InMemorySink.Instance.Should().HaveMessage("4 is not equal to 47!");
    }
}
