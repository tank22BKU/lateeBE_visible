using EvaluationService.Application.Interfaces;
using EvaluationService.Domain.Entities;
using System.Text.Json;

namespace EvaluationService.Infrastructure.ExternalServices;

public class GeminiEvaluator : IAIEvaluatorService {
    private readonly HttpClient _httpClient;
    public GeminiEvaluator(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<EpaScore>> AnalyzePerformanceAsync(EvaluationResult res) {
        var prompt = $@"Evaluate this clinical session based on:
        EPA 1 (Info Gathering): OLD CART criteria.
        EPA 2 (Diagnosis): Red flags & Differential diagnosis.
        ...
        Chat Log: {res.VpConversationLog}
        Reasoning: {res.AiReasoningLog}";

        return new List<EpaScore>(); 
    }
}