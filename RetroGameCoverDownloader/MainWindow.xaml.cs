using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBoxIcon = MsBox.Avalonia.Enums.Icon;
using RetroGameCoverDownloader.Helpers;
using RetroGameCoverDownloader.Managers;
using RetroGameCoverDownloader.Services;
using RetroGameCoverDownloader.ViewModels;
using RetroGameCoverDownloader.Views;
using Serilog;

namespace RetroGameCoverDownloader;

[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Event handlers are wired from MainWindow.axaml.")]
[SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Event handlers are wired from MainWindow.axaml.")]
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _isClosing;

    public MainWindow() : this(new MainViewModel())
    {
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Icon = AppIcons.CreateMainIcon();

        Opened += OnOpened;
        Closing += OnClosing;
        KeyDown += OnKeyDown;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        try
        {
            if (_viewModel.HasGitHubToken)
                return;

            try
            {
                var dialog = new TokenDialog();
                if (await dialog.ShowDialog<bool>(this))
                {
                    try
                    {
                        var settings = SettingsManager.LoadSettings();
                        settings.GitHubToken = dialog.Token;
                        SettingsManager.SaveSettings(settings);

                        _viewModel.UpdateToken(dialog.Token);
                        Log.Information("[MainWindow.OnOpened] GitHub token saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        await AppDialogs.ShowMessageAsync(
                            "Failed to save token. The application may have limited functionality.",
                            "Warning",
                            MessageBoxIcon.Warning);
                        Log.Error(ex, "Failed to save token after dialog.");
                    }
                }
            }
            catch (Exception ex)
            {
                await AppDialogs.ShowMessageAsync(
                    "An error occurred during startup. Some features may not work correctly.",
                    "Startup Error",
                    MessageBoxIcon.Warning);
                Log.Error(ex, "Exception during token check on window load.");
            }
        }
        catch (Exception ex)
        {
            await AppDialogs.ShowMessageAsync(
                "An error occurred during startup. Some features may not work correctly.",
                "Startup Error",
                MessageBoxIcon.Warning);
            Log.Error(ex, "Exception during token check on window load.");
        }
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isClosing) return;
        _isClosing = true;

        try
        {
            _viewModel.CancelAll();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during cancel.");
        }

        try
        {
            _viewModel.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during cleanup.");
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.F8)
            return;

        e.Handled = true;
        CaptureScreenshot();
    }

    private async void ScreenshotMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await CaptureScreenshotAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method ScreenshotMenuItem_Click");
        }
    }

    private void CaptureScreenshot()
    {
        var (success, filePath) = ScreenshotService.CaptureWindow(this);
        if (success)
        {
            Log.Information("[MainWindow] Screenshot saved: {FilePath}", filePath);
        }
        else
        {
            Log.Information("[MainWindow] Failed to save screenshot.");
            _ = AppDialogs.ShowMessageAsync(
                "Could not save the screenshot. The application data folder may not have write permissions.",
                "Screenshot Failed",
                MessageBoxIcon.Warning);
        }
    }

    private async Task CaptureScreenshotAsync()
    {
        var (success, filePath) = ScreenshotService.CaptureWindow(this);
        if (success)
        {
            Log.Information("[MainWindow] Screenshot saved: {FilePath}", filePath);
            return;
        }

        Log.Information("[MainWindow] Failed to save screenshot.");
        await AppDialogs.ShowMessageAsync(
            "Could not save the screenshot. The application data folder may not have write permissions.",
            "Screenshot Failed",
            MessageBoxIcon.Warning);
    }

    private async void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Task.CompletedTask;
            Close();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method ExitMenuItem_Click");
        }
    }

    private async void OpenAppDataPathMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var path = AppInfo.LocalAppDataFolderPath;
            Directory.CreateDirectory(path);
            PlatformHelper.OpenFolder(path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open AppData folder.");
            await AppDialogs.ShowMessageAsync(
                "Could not open the application data folder.",
                "Error",
                MessageBoxIcon.Error);
        }
    }

    private async void ProxySettingsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ProxySettingsDialog();
            if (await dialog.ShowDialog<bool>(this))
            {
                try
                {
                    var settings = SettingsManager.LoadSettings();
                    settings.UseProxy = dialog.UseProxy;
                    settings.ProxyHost = dialog.ProxyHost;
                    settings.ProxyPort = dialog.ProxyPort;
                    settings.ProxyUsername = dialog.ProxyUsername;
                    settings.ProxyPassword = dialog.ProxyPassword;
                    SettingsManager.SaveSettings(settings);

                    _viewModel.UpdateProxySettings(
                        dialog.UseProxy,
                        dialog.ProxyHost,
                        dialog.ProxyPort,
                        dialog.ProxyUsername,
                        dialog.ProxyPassword);

                    var proxyStatus = Models.AppSettings.FormatProxyStatus(
                        dialog.UseProxy, dialog.ProxyHost, dialog.ProxyPort);
                    await AppDialogs.ShowMessageAsync(
                        $"Proxy settings saved and applied. Proxy: {proxyStatus}",
                        "Proxy Settings",
                        MessageBoxIcon.Success);
                }
                catch (Exception ex)
                {
                    await AppDialogs.ShowMessageAsync(
                        "Failed to save proxy settings. Please try again.",
                        "Error",
                        MessageBoxIcon.Error);
                    Log.Error(ex, "Failed to save proxy settings.");
                }
            }
        }
        catch (Exception ex)
        {
            await AppDialogs.ShowMessageAsync(
                "An error occurred while opening proxy settings.",
                "Error",
                MessageBoxIcon.Error);
            Log.Error(ex, "Exception opening proxy settings dialog.");
        }
    }

    private async void GitHubTokenMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new TokenDialog(_viewModel.HasGitHubToken);
            if (await dialog.ShowDialog<bool>(this))
            {
                try
                {
                    var settings = SettingsManager.LoadSettings();
                    settings.GitHubToken = dialog.Token;
                    SettingsManager.SaveSettings(settings);

                    _viewModel.UpdateToken(dialog.Token);
                    Log.Information("[MainWindow.GitHubTokenMenuItem_Click] GitHub token saved successfully.");
                    await AppDialogs.ShowMessageAsync(
                        "GitHub token saved successfully.",
                        "GitHub Token",
                        MessageBoxIcon.Success);
                }
                catch (Exception ex)
                {
                    await AppDialogs.ShowMessageAsync(
                        "Failed to save token. Please try again.",
                        "Error",
                        MessageBoxIcon.Error);
                    Log.Error(ex, "Failed to save token.");
                }
            }
        }
        catch (Exception ex)
        {
            await AppDialogs.ShowMessageAsync(
                "An error occurred while opening the token dialog.",
                "Error",
                MessageBoxIcon.Error);
            Log.Error(ex, "Exception opening token dialog.");
        }
    }

    private async void FileExtensionsMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ExtensionsDialog();
            if (await dialog.ShowDialog<bool>(this))
            {
                try
                {
                    var settings = SettingsManager.LoadSettings();
                    settings.FileExtensions = dialog.FileExtensions;
                    SettingsManager.SaveSettings(settings);

                    _viewModel.UpdateFileExtensions(dialog.FileExtensions);

                    Log.Information(
                        "[MainWindow.FileExtensionsMenuItem_Click] File extensions updated. {ExtensionCount} extension(s) configured.",
                        dialog.FileExtensions.Count);
                    await AppDialogs.ShowMessageAsync(
                        $"File extensions saved. {dialog.FileExtensions.Count} extension(s) configured.",
                        "File Extensions",
                        MessageBoxIcon.Success);
                }
                catch (Exception ex)
                {
                    await AppDialogs.ShowMessageAsync(
                        "Failed to save file extensions. Please try again.",
                        "Error",
                        MessageBoxIcon.Error);
                    Log.Error(ex, "Failed to save file extensions.");
                }
            }
        }
        catch (Exception ex)
        {
            await AppDialogs.ShowMessageAsync(
                "An error occurred while opening file extensions settings.",
                "Error",
                MessageBoxIcon.Error);
            Log.Error(ex, "Exception opening extensions dialog.");
        }
    }

    private async void AboutMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await new AboutWindow().ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error opening About window: {Reason}", ex.Message);
        }
    }

    private void LogTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.CaretIndex = textBox.Text?.Length ?? 0;
        }
    }
}
