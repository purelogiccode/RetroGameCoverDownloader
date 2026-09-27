using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace RetroGameCoverDownloader.Tests;

/// <summary>
/// In-memory Serilog sink used by tests to assert which log levels a code path emits.
/// Wired into the global test logger by <see cref="TestModuleInitializer"/>.
/// </summary>
internal sealed class CollectingLogSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEvent> _events = new();

    public void Emit(LogEvent logEvent)
    {
        _events.Enqueue(logEvent);
    }

    public void Clear()
    {
        while (_events.TryDequeue(out _))
        {
        }
    }

    public bool ContainsEvent(LogEventLevel minimumLevel, string messageFragment)
    {
        return _events.Any(e =>
            e.Level >= minimumLevel
            && e.RenderMessage().Contains(messageFragment, StringComparison.Ordinal));
    }
}
