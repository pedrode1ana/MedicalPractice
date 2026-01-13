using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Library.MedicalPractice.Serialization;

namespace Library.MedicalPractice.Services;

public class WebRequestHandler
{
    private readonly string _host;
    private readonly string _port;
    private readonly HttpClient _client;

    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Converters = { new DateOnlyJsonConverter() }
    };

    public WebRequestHandler(string host = "localhost", string port = "5158")
    {
        _host = host;
        _port = port;
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    private Uri BuildUri(string path)
    {
        var baseUri = new Uri($"http://{_host}:{_port}");
        return new Uri(baseUri, path);
    }

    public async Task<T?> GetAsync<T>(string path) =>
        await SendAsync<T>(HttpMethod.Get, path);

    public async Task<T?> PostAsync<T>(string path, object? body) =>
        await SendAsync<T>(HttpMethod.Post, path, body);

    public async Task<T?> PutAsync<T>(string path, object? body) =>
        await SendAsync<T>(HttpMethod.Put, path, body);

    public async Task<bool> DeleteAsync(string path)
    {
        var response = await SendRawAsync(HttpMethod.Delete, path);
        return response?.IsSuccessStatusCode == true;
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        try
        {
            var response = await SendRawAsync(method, path, body);
            if (response is null || response.StatusCode == HttpStatusCode.NoContent)
                return default;

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(content, SerializerSettings);
        }
        catch
        {
            // Align with provided handler: swallow failures and let caller fall back
            return default;
        }
    }

    private async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string path, object? body = null)
    {
        var uri = BuildUri(path);
        try
        {
            using var request = new HttpRequestMessage(method, uri);
            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body, SerializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        }
        catch
        {
            // Hide connection errors; caller will handle defaults
            return null;
        }
    }
}
