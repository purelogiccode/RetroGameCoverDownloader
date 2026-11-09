using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroGameCoverDownloader.Helpers;
using RetroGameCoverDownloader.Services;
using Serilog;

namespace RetroGameCoverDownloader.Views;

[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Event handlers are wired from TokenDialog.axaml.")]
[SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Event handlers are wired from TokenDialog.axaml.")]
public partial class TokenDialog : Window
{
    public string Token { get; private set; } = "";
    public bool HasExistingToken { get; }

    public TokenDialog() : this(false)
    {
    }

    public TokenDialog(bool hasExistingToken)
    {
        HasExistingToken = hasExistingToken;
        InitializeComponent();

        if (HasExistingToken)
            ExistingTokenPanel.IsVisible = true;

        Opened += (_, _) => TokenBox.Focus();
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var entered = TokenBox.Text?.Trim() ?? "";
            Token = entered;
            Close(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save token.");
            _ = AppDialogs.ShowMessageAsync(
                "An error occurred while saving the token. Please try again.",
                "Error",
                MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void GitHubTokenLink_Click(object? sender, RoutedEventArgs e)
    {
        PlatformHelper.OpenUrl("https://github.com/settings/tokens/new");
    }
}
