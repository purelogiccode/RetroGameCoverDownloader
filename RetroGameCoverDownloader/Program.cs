using Avalonia;
using Serilog;

namespace RetroGameCoverDownloader;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            try
            {
                Log.Fatal(ex, "The application terminated unexpectedly.");
            }
            catch
            {
                // Logging may not be initialized yet.
            }
        }
        finally
        {
            try
            {
                Log.CloseAndFlush();
            }
            catch
            {
                // Ignore shutdown logging failures.
            }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
