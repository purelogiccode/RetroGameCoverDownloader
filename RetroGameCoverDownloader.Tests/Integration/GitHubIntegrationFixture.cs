using RetroGameCoverDownloader.Models;
using RetroGameCoverDownloader.Services;

namespace RetroGameCoverDownloader.Tests.Integration;

/// <summary>
/// Shared fixture for all GitHub integration tests.
/// Fetches the full list of libretro-thumbnails systems once per test run.
///
/// A GITHUB_TOKEN is required. The theories issue hundreds of requests, far beyond
/// GitHub's 60 requests/hour unauthenticated limit; without a token the app's own
/// RateLimiter throttles to 55 requests/hour and each over-limit call waits up to an
/// hour, which makes the suite appear to hang. Without a token the tests are skipped.
/// </summary>
public static class GitHubIntegrationFixture
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromMinutes(5);

    public static IReadOnlyList<SystemConfig> Systems { get; }
    public static string? FetchError { get; }
    public static GitHubService SharedService { get; }

    static GitHubIntegrationFixture()
    {
        try
        {
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
            {
                FetchError = "GITHUB_TOKEN is not set. The live GitHub integration tests need a token to stay within the rate limits, so they were skipped.";
                Systems = new List<SystemConfig>();
                SharedService = new GitHubService((string?)null);
                return;
            }

            SharedService = new GitHubService(token);
            Systems = SharedService.GetAvailableSystemsAsync().GetAwaiter().GetResult();
            if (Systems.Count == 0)
            {
                FetchError = "GetAvailableSystemsAsync returned an empty list.";
            }
        }
        catch (Exception ex)
        {
            FetchError = $"Exception: {ex.GetType().Name}: {ex.Message}";
            Systems = new List<SystemConfig>();
            SharedService = new GitHubService((string?)null);
        }
    }

    /// <summary>
    /// Runs a live GitHub call with a hard timeout so a rate-limit wait or stalled request
    /// can never hang the test run.
    /// </summary>
    public static async Task<T> WithTimeoutAsync<T>(Func<CancellationToken, Task<T>> action)
    {
        using var cts = new CancellationTokenSource(CallTimeout);
        return await action(cts.Token);
    }

    public static IEnumerable<object[]> GetSystems()
    {
        if (Systems.Count == 0)
        {
            yield return [new SystemConfig("SKIP", "skip", "skip", "skip"), true];

            yield break;
        }

        foreach (var system in Systems)
        {
            yield return [system, false];
        }
    }
}