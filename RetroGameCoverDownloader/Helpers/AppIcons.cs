using Avalonia.Controls;
using Avalonia.Platform;

namespace RetroGameCoverDownloader.Helpers;

public static class AppIcons
{
    private const string MainIconUri = "avares://RetroGameCoverDownloader/Assets/logo.png";

    public static WindowIcon CreateMainIcon()
    {
        return new WindowIcon(AssetLoader.Open(new Uri(MainIconUri)));
    }
}
