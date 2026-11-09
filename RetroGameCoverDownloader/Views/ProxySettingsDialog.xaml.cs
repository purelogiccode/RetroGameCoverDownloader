using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroGameCoverDownloader.Managers;
using RetroGameCoverDownloader.Services;
using Serilog;

namespace RetroGameCoverDownloader.Views;

[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Event handlers are wired from ProxySettingsDialog.axaml.")]
[SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Event handlers are wired from ProxySettingsDialog.axaml.")]
public partial class ProxySettingsDialog : Window
{
    public bool UseProxy { get; private set; }
    public string? ProxyHost { get; private set; }
    public int ProxyPort { get; private set; }
    public string? ProxyUsername { get; private set; }
    public string? ProxyPassword { get; private set; }

    public ProxySettingsDialog()
    {
        InitializeComponent();
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        try
        {
            var settings = SettingsManager.LoadSettings();
            UseProxyCheckBox.IsChecked = settings.UseProxy;
            ProxyHostBox.Text = settings.ProxyHost ?? string.Empty;
            ProxyPortBox.Text = settings.ProxyPort > 0
                ? settings.ProxyPort.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            ProxyUsernameBox.Text = settings.ProxyUsername ?? string.Empty;
            ProxyPasswordBox.Text = settings.ProxyPassword ?? string.Empty;
            ProxySettingsPanel.IsEnabled = settings.UseProxy;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load current settings.");
        }
    }

    private void UseProxyCheckBox_Changed(object? sender, RoutedEventArgs e)
    {
        ProxySettingsPanel.IsEnabled = UseProxyCheckBox.IsChecked == true;
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var parsedPort = 0;

            if (UseProxyCheckBox.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(ProxyHostBox.Text))
                {
                    await AppDialogs.ShowMessageAsync("Please enter a proxy host address.", "Validation Error", MsBox.Avalonia.Enums.Icon.Warning);
                    ProxyHostBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProxyPortBox.Text))
                {
                    await AppDialogs.ShowMessageAsync("Please enter a proxy port.", "Validation Error", MsBox.Avalonia.Enums.Icon.Warning);
                    ProxyPortBox.Focus();
                    return;
                }

                if (!int.TryParse(ProxyPortBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedPort)
                    || parsedPort <= 0
                    || parsedPort > 65535)
                {
                    await AppDialogs.ShowMessageAsync("Please enter a valid proxy port (1-65535).", "Validation Error", MsBox.Avalonia.Enums.Icon.Warning);
                    ProxyPortBox.Focus();
                    return;
                }
            }

            UseProxy = UseProxyCheckBox.IsChecked == true;
            ProxyHost = UseProxy ? ProxyHostBox.Text?.Trim() : null;
            ProxyPort = UseProxy ? parsedPort : 0;
            ProxyUsername = UseProxy && !string.IsNullOrWhiteSpace(ProxyUsernameBox.Text)
                ? ProxyUsernameBox.Text.Trim()
                : null;
            ProxyPassword = UseProxy && !string.IsNullOrEmpty(ProxyPasswordBox.Text)
                ? ProxyPasswordBox.Text
                : null;

            Close(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save proxy settings.");
            await AppDialogs.ShowMessageAsync(
                "An error occurred while saving the proxy settings. Please try again.",
                "Error",
                MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
