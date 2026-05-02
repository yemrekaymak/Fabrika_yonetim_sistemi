using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using FabrikaBackend.Services;

namespace FabrikaBackend.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly AiIntegrationService _aiIntegrationService;

    public AiController(AiIntegrationService aiIntegrationService)
    {
        _aiIntegrationService = aiIntegrationService;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AiAnalyzeRequest request, CancellationToken cancellationToken)
    {
        if (!_aiIntegrationService.TryCreateClient(out _, out var configError))
            return Problem(configError, statusCode: 500);

        var payload = new
        {
            mode = request.Mode ?? "risk_analysis",
            order_id = string.IsNullOrWhiteSpace(request.OrderId) ? null : request.OrderId.Trim(),
        };

        try
        {
            var body = await _aiIntegrationService.PostAnalyzeAsync(payload, cancellationToken);
            return Content(body, "application/json");
        }
        catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
        {
            return StatusCode((int)ex.StatusCode.Value, ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"AI servisine bağlanılamadı: {ex.Message}", statusCode: 502);
        }
    }
}

public sealed class AiAnalyzeRequest
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "risk_analysis";

    [JsonPropertyName("order_id")]
    public string? OrderId { get; set; }
}
