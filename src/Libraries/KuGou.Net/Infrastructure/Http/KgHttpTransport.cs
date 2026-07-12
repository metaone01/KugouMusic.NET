using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using KuGou.Net.Protocol.Transport;
using KuGou.Net.util;

namespace KuGou.Net.Infrastructure.Http;

public interface IKgTransport
{
    Task<JsonElement> SendAsync(KgRequest request);
}

public class KgHttpTransport(HttpClient client) : IKgTransport
{
    private const int MaxGetAttempts = 3;

    public async Task<JsonElement> SendAsync(KgRequest request)
    {
        var baseUrl = request.BaseUrl ?? "https://gateway.kugou.com";
        var urlBuilder = new StringBuilder($"{baseUrl.TrimEnd('/')}/{request.Path.TrimStart('/')}");

        if (request.Params.Count > 0)
        {
            urlBuilder.Append('?');
            var queryString = string.Join("&", request.Params.Select(kv =>
                $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            urlBuilder.Append(queryString);
        }

        var requestUrl = urlBuilder.ToString();
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await SendOnceAsync(request, requestUrl);
            }
            catch (HttpRequestException ex) when (
                request.Method == HttpMethod.Get &&
                attempt < MaxGetAttempts &&
                IsTransient(ex))
            {
                await Task.Delay(GetRetryDelay(attempt));
            }
        }
    }

    private async Task<JsonElement> SendOnceAsync(KgRequest request, string requestUrl)
    {
        using var msg = new HttpRequestMessage(request.Method, requestUrl);
        msg.Options.Set(new HttpRequestOptionsKey<KgRequest>("KgRequestDetail"), request);

        if (request.CustomHeaders != null)
            foreach (var kv in request.CustomHeaders)
                msg.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

        if (request.Method == HttpMethod.Post)
        {
            if (request.BinaryBody is { Length: > 0 })
            {
                msg.Content = new ByteArrayContent(request.BinaryBody);
                msg.Content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
            }
            else if (!string.IsNullOrEmpty(request.RawBody))
            {
                msg.Content = new StringContent(request.RawBody, Encoding.UTF8, request.ContentType);
            }
            else if (request.Body != null)
            {
                var jsonBody = RequestBodyJsonSerializer.Serialize(request.Body, request.BodyTypeInfo);
                msg.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }
        }

        using var response = await client.SendAsync(msg);
        response.EnsureSuccessStatusCode();

        var responseBytes = await response.Content.ReadAsByteArrayAsync();

        if (responseBytes.Length == 0)
        {
            using var emptyDoc = JsonDocument.Parse("{}");
            return emptyDoc.RootElement.Clone();
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBytes);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            var base64Content = Convert.ToBase64String(responseBytes);

            var fallbackJson = new JsonObject
            {
                ["__raw_base64__"] = base64Content
            };

            var fallbackDoc = JsonSerializer.SerializeToElement(fallbackJson, AppJsonContext.Default.JsonObject);
            return fallbackDoc;
        }
    }

    private static bool IsTransient(HttpRequestException exception)
    {
        if (exception.StatusCode is null)
            return true;

        var statusCode = (int)exception.StatusCode.Value;
        return statusCode is 408 or 429 || statusCode >= 500;
    }

    private static TimeSpan GetRetryDelay(int failedAttempt)
    {
        return failedAttempt == 1
            ? TimeSpan.FromMilliseconds(250)
            : TimeSpan.FromMilliseconds(750);
    }
}
