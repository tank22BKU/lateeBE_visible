using System.Text;
using System.Text.Json;
using AssessmentService.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace AssessmentService.Infrastructure.Repositories;

public class GeminiAiRepository : IGeminiAiRepository
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;

    public GeminiAiRepository(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["GEMINI_15_API_KEY"] ?? config["GeminiAi:ApiKey"];
    }

    public async Task<string> GenerateQuestionsJsonAsync(string promptInstruction, int numQuestions)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        
        var systemPrompt = $@"
                            You are a medical assessment question generator.
                            Task: Generate {numQuestions} questions based on the following instruction: {promptInstruction}.
                            Requirements:
                                - Return ONLY a valid JSON array (no markdown, no extra text).
                                - Each object must follow this structure exactly:
                                {{
                                    ""QuestionType"": ""MultipleChoice"",
                                    ""CognitiveLevel"": ""Apply"",
                                    ""Content"": ""Question content..."",
                                    ""Options"": [
                                        {{ ""id"": ""A"", ""text"": ""Option A"", ""isCorrect"": true }},
                                        {{ ""id"": ""B"", ""text"": ""Option B"", ""isCorrect"": false }}
                                    ],
                                    ""Explanation"": ""Detailed explanation of why each option is correct or incorrect.""
                                }}

                                or 

                                {{
                                    ""QuestionType"": ""TrueFalse"",
                                    ""CognitiveLevel"": ""Apply"",
                                    ""Content"": ""Question content..."",
                                    ""Options"": [
                                        {{ ""id"": ""A"", ""True"": ""Option A"", ""isCorrect"": true }},
                                        {{ ""id"": ""B"", ""False"": ""Option B"", ""isCorrect"": false }}
                                    ],
                                    ""Explanation"": ""Detailed explanation of why each option is correct or incorrect.""
                                }}
                                - Ensure:
                                    + Exactly one correct answer for MultipleChoice.
                                    + Clinical, realistic, and medically accurate content.
                                    + Clear and concise wording.
                                    + Explanations are educational and precise.
                            ";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = systemPrompt } } } },
            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var generatedText = jsonDoc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString();

        return generatedText ?? "[]";
    }

    public async Task<string> GenerateQuestionsJsonAsyncVer2(string promptInstruction, int numQuestions, string language, string? pdfContent = null)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return "[]"; 
        }

        // var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        var prompt = BuildAssessmentPrompt(promptInstruction, numQuestions, language, pdfContent);

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.2, responseMimeType = "application/json" }
        };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorContent}");
                return "[]";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);

            var generatedText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(generatedText)) return "[]";

            return ExtractJson(generatedText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GeminiAiRepository: {ex.Message}");
            return "[]";
        }
    }

    private static string BuildAssessmentPrompt(string instruction, int num, string lang, string? pdf)
    {
        var context = string.IsNullOrEmpty(pdf) ? "" : $"Base your questions strictly on this reference content: {pdf}";

        return $@"
            You are a medical assessment generator.
            Language: {lang} (Mandatory).
            Task: Generate EXACTLY {num} questions.
            
            Requirements:
            - Return ONLY a valid JSON array.
            - No markdown formatting (no ```json ... ```), no extra text, no conversational filler.
            - Each object in the array must follow this structure exactly:
            {{
                ""QuestionType"": ""MultipleChoice"",
                ""CognitiveLevel"": ""Apply"",
                ""Content"": ""Question text here..."",
                ""Options"": [
                    {{ ""id"": ""A"", ""text"": ""Option A"", ""isCorrect"": true }},
                    {{ ""id"": ""B"", ""text"": ""Option B"", ""isCorrect"": false }},
                    {{ ""id"": ""C"", ""text"": ""Option C"", ""isCorrect"": false }},
                    {{ ""id"": ""D"", ""text"": ""Option D"", ""isCorrect"": false }}
                ],
                ""Explanation"": ""Detailed educational explanation...""
            }}

            Rules:
            - Exactly one correct answer per question.
            - Content must be clinical, realistic, and medically accurate.
            - Ensure clear and concise wording.

            Instruction: {instruction}
            {context}
        ";
    }

    private static string ExtractJson(string raw)
    {
        var text = raw.Trim();

        if (text.StartsWith("```") && text.Contains("\n"))
        {
            var firstNewline = text.IndexOf('\n');
            var lastFence = text.LastIndexOf("```");
            if (firstNewline >= 0 && lastFence > firstNewline)
            {
                text = text.Substring(firstNewline + 1, lastFence - firstNewline - 1).Trim();
            }
        }

        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        if (start >= 0 && end > start)
        {
            return text.Substring(start, end - start + 1);
        }

        return text;
    }
}