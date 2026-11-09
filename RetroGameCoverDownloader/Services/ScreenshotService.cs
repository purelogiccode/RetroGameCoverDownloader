using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using RetroGameCoverDownloader.Helpers;

namespace RetroGameCoverDownloader.Services;

public static class ScreenshotService
{
    private static readonly string ScreenshotsFolder = AppInfo.ScreenshotsFolderPath;

    /// <summary>
    /// Captures the application window as a PNG. Rendering the window through
    /// Avalonia works on Windows, Linux, and macOS without platform-specific APIs.
    /// Must be called on the UI thread.
    /// </summary>
    public static (bool Success, string? FilePath) CaptureWindow(Window window)
    {
        try
        {
            Directory.CreateDirectory(ScreenshotsFolder);

            var scale = window.RenderScaling;
            var width = (int)Math.Ceiling(window.Bounds.Width * scale);
            var height = (int)Math.Ceiling(window.Bounds.Height * scale);

            if (width <= 0 || height <= 0)
                return (false, null);

            using var bitmap = new RenderTargetBitmap(
                new PixelSize(width, height),
                new Vector(96 * scale, 96 * scale));

            bitmap.Render(window);

            var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
            var filePath = Path.Combine(ScreenshotsFolder, fileName);
            bitmap.Save(filePath, PngBitmapEncoderOptions.Default);

            return (true, filePath);
        }
        catch
        {
            return (false, null);
        }
    }
}
