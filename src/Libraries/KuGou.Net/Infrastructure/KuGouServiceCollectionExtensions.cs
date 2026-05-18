using System.Net;
using Jab;
using KuGou.Net.Clients;
using KuGou.Net.Infrastructure.Http;
using KuGou.Net.Infrastructure.Http.Handlers;
using KuGou.Net.Protocol.Raw;
using KuGou.Net.Protocol.Session;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace KuGou.Net.Infrastructure;

[ServiceProviderModule]
[Transient<RawSearchApi>]
[Transient<RawLoginApi>]
[Transient<RawPlaylistApi>]
[Transient<RawUserApi>]
[Transient<RawDeviceApi>]
[Transient<RawLyricApi>]
[Transient<RawRankApi>]
[Transient<RawAlbumApi>]
[Transient<RawSongApi>]
[Transient<RawArtistApi>]
[Transient<RawCommentApi>]
[Transient<RawFmApi>]
[Transient<RawMediaCatalogApi>]
[Transient<RawReportApi>]
[Transient<RawDiscoveryApi>]
[Transient<RecommendClient>]
[Transient<RankClient>]
[Transient<SearchClient>]
[Transient<LoginClient>]
[Transient<PlaylistClient>]
[Transient<UserClient>]
[Transient<RegisterClient>]
[Transient<LyricClient>]
[Transient<AlbumClient>]
[Transient<SongClient>]
[Transient<ArtistClient>]
[Transient<CommentClient>]
[Transient<FmClient>]
[Transient<VideoClient>]
[Transient<LongAudioClient>]
[Transient<IpClient>]
[Transient<SceneClient>]
[Transient<ThemeClient>]
[Transient<ReportClient>]
public interface IKuGouServiceModule;

[ServiceProvider]
[Import<IKuGouServiceModule>]
[Singleton<ISessionPersistence>(Instance = nameof(SessionPersistence))]
[Singleton<CookieContainer>(Instance = nameof(CookieContainer))]
[Singleton<ILoggerFactory>(Instance = nameof(LoggerFactory))]
[Singleton<KgSessionManager>]
[Transient<KgSignatureHandler>]
[Transient<IKgTransport>(Factory = nameof(CreateTransport))]
[Singleton<ILogger<RawLoginApi>>(Factory = nameof(CreateRawLoginApiLogger))]
[Singleton<ILogger<RawPlaylistApi>>(Factory = nameof(CreateRawPlaylistApiLogger))]
[Singleton<ILogger<RawDeviceApi>>(Factory = nameof(CreateRawDeviceApiLogger))]
[Singleton<ILogger<LoginClient>>(Factory = nameof(CreateLoginClientLogger))]
[Singleton<ILogger<RegisterClient>>(Factory = nameof(CreateRegisterClientLogger))]
public sealed partial class KuGouServiceProvider
{
    public ISessionPersistence SessionPersistence { get; set; } = new InMemorySessionPersistence();

    public CookieContainer CookieContainer { get; set; } = new();

    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

    public IKgTransport CreateTransport(CookieContainer cookieContainer, KgSignatureHandler signatureHandler)
    {
        var primaryHandler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        signatureHandler.InnerHandler = primaryHandler;

        var client = new HttpClient(signatureHandler, disposeHandler: true);
        return new KgHttpTransport(client);
    }

    public ILogger<RawLoginApi> CreateRawLoginApiLogger() => LoggerFactory.CreateLogger<RawLoginApi>();

    public ILogger<RawPlaylistApi> CreateRawPlaylistApiLogger() => LoggerFactory.CreateLogger<RawPlaylistApi>();

    public ILogger<RawDeviceApi> CreateRawDeviceApiLogger() => LoggerFactory.CreateLogger<RawDeviceApi>();

    public ILogger<LoginClient> CreateLoginClientLogger() => LoggerFactory.CreateLogger<LoginClient>();

    public ILogger<RegisterClient> CreateRegisterClientLogger() => LoggerFactory.CreateLogger<RegisterClient>();
}
