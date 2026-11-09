using RetroGameCoverDownloader.Services;

namespace RetroGameCoverDownloader.Tests.Services;

internal class MockBugReportService : IBugReportService
{
    // Intentionally does nothing: the real service writes logs and calls a remote endpoint.
    public void LogErrorSync(Exception? ex, string? contextMessage = null)
    {
    }

    public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
    {
        return Task.CompletedTask;
    }
}
