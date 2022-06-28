using AutoFixture;
using AutoFixture.AutoMoq;

namespace Serilog.Sinks.FingersCrossed.Tests;

internal class AutoDataAttribute : AutoFixture.Xunit2.AutoDataAttribute
{
    public AutoDataAttribute() : base(() => new Fixture()
        .Customize(new AutoMoqCustomization
        {
            ConfigureMembers = true,
            GenerateDelegates = true
        })
        .Customize(new LogEventGenerator()))
    {
    }
}
