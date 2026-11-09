using System.Net;
using System.Text.Json;
using RetroGameCoverDownloader.Helpers;
using RetroGameCoverDownloader.Services;
using Serilog.Events;
using Xunit;

namespace RetroGameCoverDownloader.Tests.Services;

public class ApplicationStatsServiceTests
{
    #region TrackLaunchAsync Tests

    [Fact]
    public async Task TrackLaunchAsyncSendsCorrectPayload()
    {
        var handler = new TrackingHttpMessageHandler();
        var testClient = new HttpClient(handler);

        var testService = new ApplicationStatsService { HttpClientFactory = () => testClient };
        var original = ApplicationStatsService.Current;
        ApplicationStatsService.Current = testService;

        try
        {
            await ApplicationStatsService.TrackLaunchAsync();

            Assert.NotNull(handler.CapturedRequest);
            Assert.Equal(HttpMethod.Post, handler.CapturedRequest!.Method);
            Assert.Equal("https://www.purelogiccode.com/ApplicationStats/stats", handler.CapturedRequest.RequestUri!.ToString());

            Assert.NotNull(handler.CapturedRequest.Headers.Authorization);
            Assert.Equal("Bearer", handler.CapturedRequest.Headers.Authorization!.Scheme);
            Assert.False(string.IsNullOrEmpty(handler.CapturedRequest.Headers.Authorization.Parameter));

            Assert.NotNull(handler.CapturedRequest.Content);
            Assert.Equal("application/json", handler.CapturedRequest.Content!.Headers.ContentType!.MediaType);

            var body = await handler.CapturedRequest.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            Assert.True(doc.RootElement.TryGetProperty("applicationId", out var appId));
            Assert.Equal("retro-game-cover-downloader", appId.GetString());

            Assert.True(doc.RootElement.TryGetProperty("version", out var version));
            Assert.Equal(AppInfo.VersionString, version.GetString());
        }
        finally
        {
            ApplicationStatsService.Current = original;
        }
    }

    [Fact]
    public async Task TrackLaunchAsyncHandlesNetworkErrorsGracefully()
    {
        var handler = new FaultingHttpMessageHandler();
        var testClient = new HttpClient(handler);

        var testService = new ApplicationStatsService { HttpClientFactory = () => testClient };
        var original = ApplicationStatsService.Current;
        ApplicationStatsService.Current = testService;

        try
        {
            var ex = await Record.ExceptionAsync(static () => ApplicationStatsService.TrackLaunchAsync());

            Assert.Null(ex);
        }
        finally
        {
            ApplicationStatsService.Current = original;
        }
    }

    [Fact]
    public async Task TrackLaunchAsyncTimeoutLogsInformationNotError()
    {
        TestModuleInitializer.LogSink.Clear();

        var timeout = new TaskCanceledException(
            "The request was canceled due to the configured HttpClient.Timeout of 15 seconds elapsing.",
            new TimeoutException());
        var handler = new ThrowingHttpMessageHandler(timeout);
        var testClient = new HttpClient(handler);

        var testService = new ApplicationStatsService { HttpClientFactory = () => testClient };
        var original = ApplicationStatsService.Current;
        ApplicationStatsService.Current = testService;

        try
        {
            var ex = await Record.ExceptionAsync(static () => ApplicationStatsService.TrackLaunchAsync());

            Assert.Null(ex);
            Assert.True(
                TestModuleInitializer.LogSink.ContainsEvent(LogEventLevel.Information, "Launch telemetry skipped"),
                "A telemetry timeout should be logged as an informational message.");
            Assert.False(
                TestModuleInitializer.LogSink.ContainsEvent(LogEventLevel.Error, "Launch telemetry"),
                "A telemetry timeout must not be logged as an error that is forwarded to the bug-report API.");
        }
        finally
        {
            ApplicationStatsService.Current = original;
        }
    }

    #endregion

    #region Helpers

    private sealed class TrackingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class FaultingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated network failure");
        }
    }

    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw exception;
        }
    }

    #endregion
}
