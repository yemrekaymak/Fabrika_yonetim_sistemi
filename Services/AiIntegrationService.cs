using System.Net.Http.Json;

namespace FabrikaBackend.Services;

public sealed class AiIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiIntegrationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public string? GetBaseUrl()
    {
        var envUrl = Environment.GetEnvironmentVariable("AI_SERVICE_BASE_URL");
        var configUrl = _configuration["AiService:BaseUrl"];
        var value = string.IsNullOrWhiteSpace(envUrl) ? configUrl : envUrl;
        return value?.Trim().TrimEnd('/');
    }

    public bool TryCreateClient(out HttpClient client, out string? errorMessage)
    {
        client = _httpClientFactory.CreateClient();

        var baseUrl = GetBaseUrl();
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            errorMessage = "AI_SERVICE_BASE_URL veya AiService:BaseUrl ayarlı değil.";
            return false;
        }

        client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(60);
        errorMessage = null;
        return true;
    }

    public async Task<string> PostAnalyzeAsync(object payload, CancellationToken cancellationToken)
    {
        if (!TryCreateClient(out var client, out var errorMessage))
        {
            throw new InvalidOperationException(errorMessage);
        }

        var response = await client.PostAsJsonAsync("/api/ai/analyze", payload, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"AI servisi hata döndü ({(int)response.StatusCode}): {body}",
                null,
                response.StatusCode
            );
        }

        return body;
    }
}
