using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KuGou.Net.Clients;
using KuGou.Net.Protocol.Session;
using KugouAvaloniaPlayer.Services.Jellyfin;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Services;

public interface IPlaybackSourceResolver
{
    Task<PlaybackSourceResult> ResolveAsync(SongItem song, string quality, CancellationToken cancellationToken);
}

public sealed class PlaybackSourceResolver(
    SongClient songClient,
    KgSessionManager sessionManager,
    IJellyfinClient jellyfinClient)
    : IPlaybackSourceResolver
{
    public async Task<PlaybackSourceResult> ResolveAsync(
        SongItem song,
        string quality,
        CancellationToken cancellationToken)
    {
        var localFilePath = song.LocalFilePath;
        if (string.Equals(song.LocalSourceType, LocalMusicLibraryService.SourceTypeJellyfin, StringComparison.Ordinal) ||
            (!string.IsNullOrWhiteSpace(localFilePath) &&
             localFilePath.StartsWith("jellyfin://", StringComparison.OrdinalIgnoreCase)))
        {
            var streamUrl = ResolveJellyfinStreamUrl(song.RemoteUrl, localFilePath);
            return string.IsNullOrWhiteSpace(streamUrl)
                ? PlaybackSourceResult.Failed(PlaybackSourceFailureReason.Unavailable)
                : PlaybackSourceResult.Remote(streamUrl);
        }

        if (!string.IsNullOrWhiteSpace(localFilePath) && File.Exists(localFilePath))
            return PlaybackSourceResult.Local(localFilePath);

        if (string.IsNullOrEmpty(sessionManager.Session.Token) || sessionManager.Session.UserId == "0")
            return PlaybackSourceResult.Failed(PlaybackSourceFailureReason.LoginRequired);

        cancellationToken.ThrowIfCancellationRequested();
        var playData = await songClient.GetPlayInfoAsync(song.Hash, quality);
        cancellationToken.ThrowIfCancellationRequested();

        if (playData == null || playData.Status != 1)
            return PlaybackSourceResult.Failed(PlaybackSourceFailureReason.Unavailable);

        var url = playData.Urls?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return string.IsNullOrWhiteSpace(url)
            ? PlaybackSourceResult.Failed(PlaybackSourceFailureReason.EmptyUrl)
            : PlaybackSourceResult.Remote(url);
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private string? ResolveJellyfinStreamUrl(string? cachedStreamUrl, string? source)
    {
        if (!string.IsNullOrWhiteSpace(cachedStreamUrl))
            return cachedStreamUrl;

        if (IsHttpUrl(source))
            return source;

        return string.IsNullOrWhiteSpace(source)
            ? null
            : jellyfinClient.BuildStreamUrlFromPseudoPath(source);
    }
}

public sealed record PlaybackSourceResult(
    bool Success,
    string? Source,
    bool IsLocal,
    PlaybackSourceFailureReason FailureReason)
{
    public static PlaybackSourceResult Local(string source) =>
        new(true, source, true, PlaybackSourceFailureReason.None);

    public static PlaybackSourceResult Remote(string source) =>
        new(true, source, false, PlaybackSourceFailureReason.None);

    public static PlaybackSourceResult Failed(PlaybackSourceFailureReason reason) =>
        new(false, null, false, reason);
}

public enum PlaybackSourceFailureReason
{
    None,
    LoginRequired,
    Unavailable,
    EmptyUrl
}
