using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroGameCoverDownloader.Helpers;

namespace RetroGameCoverDownloader.Views;

[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Event handlers are wired from AboutWindow.axaml.")]
[SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Event handlers are wired from AboutWindow.axaml.")]
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        AppVersionTextBlock.Text = $"Version: {AppInfo.VersionString}";
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void WebsiteButton_Click(object? sender, RoutedEventArgs e)
    {
        PlatformHelper.OpenUrl("https://www.purelogiccode.com");
    }
}
