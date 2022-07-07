using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Serilog.Sinks.FingersCrossed;

/// <summary>
/// Buffer scope manager.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class LogBuffer
{
    private static readonly LogBuffer Instance = new();

    internal readonly AsyncLocal<ScopeBuffer?> Scope = new();

    internal LogBuffer()
    {
    }

    internal static ScopeBuffer? GetCurrentBuffer() => Instance.Scope.Value;

    /// <summary>
    /// Starts a new scope in which all the messages will be queued and flushed only when a certain log level is seen.
    /// </summary>
    /// <returns>An object that determines the scope's boundaries</returns>
    public static IScopeBuffer BeginScope() => new ScopeBuffer(Instance);
}
