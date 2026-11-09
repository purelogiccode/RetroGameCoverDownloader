using System.Reflection;

namespace RetroGameCoverDownloader.Helpers;

public static class AppInfo
{
    private static readonly Version? VersionCache = Assembly.GetExecutingAssembly().GetName().Version;

    public const string AppName = "RetroGameCoverDownloader";

    public static Version Version => VersionCache ?? new Version(0, 0, 0, 0);

    public static string VersionString => VersionCache?.ToString() ?? "Unknown";

    public static string LocalAppDataFolderPath { get; } = Path.Combine(GetAppDataRoot(), AppName);

    private static string GetAppDataRoot()
    {
        if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Application Support");
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public static string LogsFolderPath { get; } = Path.Combine(LocalAppDataFolderPath, "logs");

    public static string ScreenshotsFolderPath { get; } = Path.Combine(LocalAppDataFolderPath, "Screenshots");
}
