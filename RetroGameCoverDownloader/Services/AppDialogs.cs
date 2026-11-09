using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace RetroGameCoverDownloader.Services;

/// <summary>
/// Small cross-platform dialog helper shared by the main window and the view models.
/// </summary>
public static class AppDialogs
{
    public static Window? MainWindow =>
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    public static async Task ShowMessageAsync(
        string message,
        string title = "Retro Game Cover Downloader",
        Icon icon = Icon.Info)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);

        if (MainWindow is { IsVisible: true } owner)
            await box.ShowWindowDialogAsync(owner);
        else
            await box.ShowAsync();
    }

    public static async Task<bool> ShowConfirmAsync(
        string message,
        string title = "Retro Game Cover Downloader")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo, Icon.Question);
        var result = MainWindow is { IsVisible: true } owner
            ? await box.ShowWindowDialogAsync(owner)
            : await box.ShowAsync();

        return result == ButtonResult.Yes;
    }
}
