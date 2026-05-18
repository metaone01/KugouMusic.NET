using System.Net.Http;

namespace KugouAvaloniaPlayer.Services;

public sealed class SimpleHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        return new HttpClient();
    }
}
