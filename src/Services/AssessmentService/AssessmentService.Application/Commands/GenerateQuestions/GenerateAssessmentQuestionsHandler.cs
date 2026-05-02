using MediatR;
using System.Text.Json;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;
using AssessmentService.Application.DTOs;

namespace AssessmentService.Application.Commands.GenerateQuestions;

public record GenerateAssessmentQuestionsCommand(string AssessmentId, string Prompt) : IRequest<bool>;

public class GenerateAssessmentQuestionsHandler : IRequestHandler<GenerateAssessmentQuestionsCommand, bool>
{
    private readonly IAssessmentRepository _repo;
    private readonly IGeminiAiRepository _aiRepository; 

    public GenerateAssessmentQuestionsHandler(IAssessmentRepository repo, IGeminiAiRepository aiRepository)
    {
        _repo = repo;
        _aiRepository = aiRepository;
    }

    public async Task<bool> Handle(GenerateAssessmentQuestionsCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _repo.GetByIdAsync(request.AssessmentId);
        if (assessment == null) return false;

        // var fullPrompt = $"Topic: {assessment.Topic}. Subtopic: {assessment.Subtopic}. " +
        //                 $"Difficulty: {assessment.DifficultyLevel}. Specialty: {assessment.Specialty}. " +
        //                 $"Additional request: {request.Prompt}";
        var specificFocus = string.IsNullOrWhiteSpace(assessment.Subtopic) 
            ? assessment.Topic 
            : $"{assessment.Subtopic} (within the context of {assessment.Topic})";

        var learningGoal = string.IsNullOrWhiteSpace(assessment.Goal) 
            ? "Evaluate clinical reasoning and diagnostic skills" 
            : assessment.Goal;

        var fullPrompt = $@"
            Generate EXACTLY {assessment.NumQuestions} {assessment.DifficultyLevel.ToLower()}, case-based questions focusing on {specificFocus}. 
            Each question MUST start with a realistic clinical vignette (including patient age, presentation, vital signs, and key lab/imaging results) relevant to the specialty of {assessment.Specialty}. 
            Focus on achieving the following assessment goal: {learningGoal}. 
            Ensure all diagnostic and treatment scenarios align with current {assessment.Specialty} guidelines. 
            Ensure distractors (incorrect options) represent common clinical pitfalls, unsafe practices, or misconceptions in this field.
            Question types accepted: {string.Join(", ", assessment.AllowedQuestionTypes)}.
            Additional specific instructions from the creator: {request.Prompt}
        ";
    var jsonResponse = await _aiRepository.GenerateQuestionsJsonAsync(fullPrompt, assessment.NumQuestions);
    if (string.IsNullOrWhiteSpace(jsonResponse)) return false;

    var cleanedJson = jsonResponse.Trim();
    
    if (cleanedJson.StartsWith("```"))
    {
        int firstNewLine = cleanedJson.IndexOf('\n');
        int lastBackticks = cleanedJson.LastIndexOf("```");
        if (firstNewLine != -1 && lastBackticks > firstNewLine)
        {
            cleanedJson = cleanedJson.Substring(firstNewLine, lastBackticks - firstNewLine).Trim();
        }
        else
        {
            cleanedJson = cleanedJson.Replace("```json", "").Replace("```", "").Trim();
        }
    }

    // cleanedJson = cleanedJson.Replace("\n", "\\n").Replace("\r", "\\r");

    List<GeneratedQuestionDto>? generatedQuestions;
    try
    {
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true, 
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(cleanedJson, options);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"JSON Deserialization failed: {ex.Message}");
        return false;
    }

        // var generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(jsonResponse, 
        //     new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (generatedQuestions == null || !generatedQuestions.Any()) return false;

        var entities = generatedQuestions.Select(q => new AssessmentQuestion
        {
            AssessmentId = assessment.AssessmentId,
            QuestionType = q.QuestionType,
            CognitiveLevel = q.CognitiveLevel,
            Content = q.Content,
            Options = JsonSerializer.Serialize(q.Options), 
            Explanation = q.Explanation,
            Points = 1.0m,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _repo.AddQuestionsAsync(entities);
        
        assessment.GenerationPrompt = request.Prompt;
        assessment.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(assessment);

        return true;
    }
}