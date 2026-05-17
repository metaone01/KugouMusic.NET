using KuGou.Net.Infrastructure.Http;
using KuGou.Net.Infrastructure.Http.Handlers;
using KuGou.Net.Protocol.Transport;
using System.Net;
using System.Text.Json;

namespace KgWebApi.Net.Services;

public sealed class WebApiKgTransport : IKgTransport, IDisposable
{
    private readonly HttpClient _client;
    private readonly KgHttpTransport _transport;

    public WebApiKgTransport(CookieContainer cookieContainer, KgSignatureHandler signatureHandler)
    {
        signatureHandler.InnerHandler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookieContainer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _client = new HttpClient(signatureHandler, disposeHandler: true);
        _transport = new KgHttpTransport(_client);
    }

    public Task<JsonElement> SendAsync(KgRequest request)
    {
        return _transport.SendAsync(request);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
