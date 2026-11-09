using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using RetroGameCoverDownloader.Helpers;
using RetroGameCoverDownloader.Managers;
using RetroGameCoverDownloader.Services;
using Serilog;

namespace RetroGameCoverDownloader.Views;

[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Event handlers are wired from ExtensionsDialog.axaml.")]
[SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Event handlers are wired from ExtensionsDialog.axaml.")]
public partial class ExtensionsDialog : Window
{
    private readonly ObservableCollection<string> _extensions = [];

    public List<string> FileExtensions { get; private set; } = [];

    public ExtensionsDialog()
    {
        InitializeComponent();
        Icon = AppIcons.CreateMainIcon();
        ExtensionsListBox.ItemsSource = _extensions;
        LoadCurrentExtensions();
    }

    private void LoadCurrentExtensions()
    {
        try
        {
            var settings = SettingsManager.LoadSettings();
            FileExtensions = settings.FileExtensions.ToList();
            _extensions.Clear();

            foreach (var extension in FileExtensions.OrderBy(static e => e, StringComparer.OrdinalIgnoreCase))
                _extensions.Add(extension);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load current extensions.");
        }
    }

    private async void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var input = NewExtensionBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(input))
            {
                await AppDialogs.ShowMessageAsync("Please enter a file extension.", "Validation Error", MsBox.Avalonia.Enums.Icon.Warning);
                NewExtensionBox.Focus();
                return;
            }

            if (!input.StartsWith('.'))
                input = "." + input;

            input = input.ToLowerInvariant();

            if (_extensions.Any(ext => string.Equals(ext, input, StringComparison.OrdinalIgnoreCase)))
            {
                await AppDialogs.ShowMessageAsync("This extension is already in the list.", "Duplicate");
                NewExtensionBox.Focus();
                return;
            }

            _extensions.Add(input);
            NewExtensionBox.Text = "";
            NewExtensionBox.Focus();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method AddButton_Click");
        }
    }

    private async void RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (ExtensionsListBox.SelectedItem is string selected)
            {
                _extensions.Remove(selected);
                return;
            }

            await AppDialogs.ShowMessageAsync("Please select an extension to remove.", "No Selection");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method RemoveButton_Click");
        }
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        _extensions.Clear();
        foreach (var extension in Models.AppSettings.DefaultExtensions.OrderBy(static x => x, StringComparer.OrdinalIgnoreCase))
            _extensions.Add(extension);
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            FileExtensions = _extensions.ToList();
            Close(true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save extensions.");
            _ = AppDialogs.ShowMessageAsync(
                "An error occurred while saving. Please try again.",
                "Error",
                MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
