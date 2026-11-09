using System.Diagnostics;
using Serilog;

namespace RetroGameCoverDownloader.Helpers;

/// <summary>
/// Cross-platform shell helpers for opening URLs and folders.
/// </summary>
public static class PlatformHelper
{
    public static bool OpenUrl(string url)
    {
        return Open(url, "URL");
    }

    public static bool OpenFolder(string path)
    {
        return Open(path, "folder");
    }

    private static bool Open(string target, string kind)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                return true;
            }

            var command = OperatingSystem.IsMacOS() ? "open" : "xdg-open";
            var startInfo = new ProcessStartInfo(command)
            {
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(target);
            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open {Kind} '{Target}'.", kind, target);
            return false;
        }
    }
}
