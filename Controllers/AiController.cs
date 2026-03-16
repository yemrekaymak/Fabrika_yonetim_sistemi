using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Data;
using FabrikaBackend.Services;

namespace FabrikaBackend.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, AppDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AiAnalyzeRequest request, CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["AiService:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            return Problem("AiService:BaseUrl ayarlı değil (appsettings.json).", statusCode: 500);

        object payload;
        try
        {
            var factoryState = await AiStateBuilder.BuildFactoryStateAsync(_context, cancellationToken);
            payload = new
            {
                mode = request.Mode ?? "risk_analysis",
                order_id = request.OrderId,
                factory_state = factoryState
            };
        }
        catch (Exception ex)
        {
            return Problem($"Fabrika verisi hazırlanamadı: {ex.Message}", statusCode: 500);
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        client.Timeout = TimeSpan.FromSeconds(60);

        HttpResponseMessage res;
        try
        {
            res = await client.PostAsJsonAsync("/api/ai/analyze", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            return Problem($"AI servisine bağlanılamadı: {ex.Message}", statusCode: 502);
        }

        var body = await res.Content.ReadAsStringAsync(cancellationToken);
        if (!res.IsSuccessStatusCode)
            return StatusCode((int)res.StatusCode, body);

        return Content(body, "application/json");
    }
}

public sealed class AiAnalyzeRequest
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "risk_analysis";

    [JsonPropertyName("order_id")]
    public int? OrderId { get; set; }
}

