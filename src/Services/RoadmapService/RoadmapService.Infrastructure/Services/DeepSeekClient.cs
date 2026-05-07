using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class HuggingFaceDeepSeekClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<HuggingFaceDeepSeekClient> _logger;
    
    private const string SystemPrompt = """
                                        You are a medical learning roadmap generator for abdominal pathology.
                                        Rules:
                                        - Return ONLY valid JSON matching the schema provided.
                                        - No markdown, no explanation, no preamble.
                                        - Do NOT show your reasoning process.
                                        - Be concise. Each field must be under 80 words.
                                        """;

    public HuggingFaceDeepSeekClient(HttpClient httpClient, IConfiguration config, ILogger<HuggingFaceDeepSeekClient> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["HuggingFace:ApiKey"];
        _baseUrl = config["HuggingFace:BaseUrl"];
        _model = config["HuggingFace:Model"];
        _logger = logger;
    }

    public async Task<string> ChatAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 2000,
            temperature = 0.2 
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/chat/completions"
        );

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();
        
        _logger.LogInformation("HF response: {body}", body);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"HF DeepSeek error: {response.StatusCode} - {body}");
        }

        return body;
    }
    
    public async Task<string> ChatAsyncVer2(string userPrompt,
        CancellationToken ct = default)
    {
        var requestBody = new
        {
            model    = _model,
            messages = new[]
            {
                // Tách system / user — giảm reasoning về instruction
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = userPrompt   }
            },
            temperature        = 0.0,
            max_tokens = 2000,
            response_format    = new { type = "json_object" },
            // ── DeepSeek-specific: giới hạn thinking budget ─────────
            // Nếu HuggingFace endpoint hỗ trợ (kiểm tra docs của provider)
            // thinking = new { type = "enabled", budget_tokens = 800 }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_baseUrl}/chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("DeepSeek request timed out after 55s");
            throw new TimeoutException("DeepSeek V4 Pro did not respond within 55 seconds.");
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("HF response status={Status} body={Body}",
            response.StatusCode, body);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"HF DeepSeek error: {response.StatusCode} — {body}");

        return body;
    }
}