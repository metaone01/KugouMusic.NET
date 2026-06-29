using KuGou.Net.Clients;
using KugouAvaloniaPlayer.Services.DesktopLyric;
using Microsoft.Extensions.Logging;
using SukiUI.Toasts;

namespace KugouAvaloniaPlayer.ViewModels;

public interface ISingerViewModelFactory
{
    SingerViewModel Create(string authorId, string singerName);
}

public sealed class SingerViewModelFactory(
    ArtistClient artistClient,
    AlbumClient albumClient,
    PlaylistClient playlistClient,
    ISukiToastManager toastManager,
    ILogger<SingerViewModel> logger)
    : ISingerViewModelFactory
{
    public SingerViewModel Create(string authorId, string singerName)
    {
        return new SingerViewModel(
            artistClient,
            albumClient,
            playlistClient,
            toastManager,
            logger,
            authorId,
            singerName);
    }
}

public interface IDiscoverTagViewModelFactory
{
    DiscoverTagViewModel Create();
}

public sealed class DiscoverTagViewModelFactory(
    PlaylistClient playlistClient,
    RecommendClient discoveryClient,
    KugouAvaloniaPlayer.Services.INavigationService navigationService,
    ISukiToastManager toastManager,
    ILogger<DiscoverTagViewModel> logger)
    : IDiscoverTagViewModelFactory
{
    public DiscoverTagViewModel Create()
    {
        return new DiscoverTagViewModel(
            playlistClient,
            discoveryClient,
            navigationService,
            toastManager,
            logger);
    }
}

public interface IDesktopLyricViewModelFactory
{
    DesktopLyricViewModel Create();
}

public sealed class DesktopLyricViewModelFactory(
    PlayerViewModel playerViewModel,
    IDesktopLyricMousePassthroughService desktopLyricMousePassthroughService)
    : IDesktopLyricViewModelFactory
{
    public DesktopLyricViewModel Create()
    {
        return new DesktopLyricViewModel(
            playerViewModel,
            desktopLyricMousePassthroughService.IsSupported,
            usesSeparateLockOverlay: !desktopLyricMousePassthroughService.SupportsSelectiveHitTesting);
    }
}
