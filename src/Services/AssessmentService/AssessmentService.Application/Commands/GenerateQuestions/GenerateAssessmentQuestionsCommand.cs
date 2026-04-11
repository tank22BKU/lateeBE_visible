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

        var fullPrompt = $"Topic: {assessment.Topic}. Subtopic: {assessment.Subtopic}. " +
                        $"Difficulty: {assessment.DifficultyLevel}. Specialty: {assessment.Specialty}. " +
                        $"Additional request: {request.Prompt}";


        var jsonResponse = await _aiRepository.GenerateQuestionsJsonAsync(fullPrompt, assessment.NumQuestions);


        var generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(jsonResponse, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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