using MediatR;
using System.Text.Json;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repositories;
using AssessmentService.Application.DTOs;

namespace AssessmentService.Application.Commands.CreateFullAssessment;

public record CreateFullAssessmentCommand(
    string CreatorId, string Title, string Specialty, string Topic, 
    string DifficultyLevel, string Goal, string Descriptions,
    int NumQuestions, int TimeLimitMinutes, decimal PassingScorePercentage,
    int MaxAttempts, string GenerationPrompt, string Language = "English", 
    string? PdfFileName = null
) : IRequest<object>;

public class CreateFullAssessmentHandler: IRequestHandler<CreateFullAssessmentCommand, object>
{
    private readonly IAssessmentRepository _repo;
    private readonly IGeminiAiRepository _aiRepo;

    public CreateFullAssessmentHandler(IAssessmentRepository repo, IGeminiAiRepository aiRepo)
    {
        _repo = repo;
        _aiRepo = aiRepo;
    }

    public async Task<object> Handle(CreateFullAssessmentCommand request, CancellationToken cancellationToken)
    {
        var comprehensivePrompt = $@"
            Context for the assessment:
            - Title: {request.Title}
            - Specialty: {request.Specialty}
            - Topic: {request.Topic}
            - Difficulty Level: {request.DifficultyLevel}
            - Learning Goal: {request.Goal}
            - Module Description: {request.Descriptions}

            Specific Instructions from user:
            {request.GenerationPrompt}
        ";

        string? pdfText = null;
        if (!string.IsNullOrEmpty(request.PdfFileName))
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pdfs", request.PdfFileName);
            if (File.Exists(path))
            {
                pdfText = ExtractTextFromPdf(path);
            }
        }
        if (string.IsNullOrWhiteSpace(pdfText))
        {
            var defaultPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pdfs", "cardiologyFile.pdf");
            if (File.Exists(defaultPdfPath))
            {
                pdfText = ExtractTextFromPdf(defaultPdfPath);
            }
        }

        var aiResponse = await _aiRepo.GenerateQuestionsJsonAsyncVer2(
            comprehensivePrompt, 
            request.NumQuestions, 
            request.Language, 
            pdfText);

        var generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestionDto>>(aiResponse, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (generatedQuestions == null || !generatedQuestions.Any())
        {
            throw new InvalidOperationException("Gemini AI failed to generate questions. Please check your API Key, Quota, or ensure the prompt is valid.");
        }

        var assessment = new Assessment
        {
            AssessmentId = Guid.NewGuid().ToString("N"),
            CreatorId = request.CreatorId,
            Title = request.Title,
            Specialty = request.Specialty,
            Topic = request.Topic,
            DifficultyLevel = request.DifficultyLevel,
            Goal = request.Goal,
            Descriptions = request.Descriptions,
            NumQuestions = generatedQuestions.Count, 
            TimeLimitMinutes = request.TimeLimitMinutes,
            PassingScorePercentage = request.PassingScorePercentage,
            MaxAttempts = request.MaxAttempts,
            GenerationPrompt = request.GenerationPrompt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(assessment);

        var questionEntities = generatedQuestions.Select(q => new AssessmentQuestion
        {
            QuestionId = Guid.NewGuid().ToString("N"),
            AssessmentId = assessment.AssessmentId,
            QuestionType = q.QuestionType ?? "MultipleChoice",
            Content = q.Content,
            Options = JsonSerializer.Serialize(q.Options),
            Explanation = q.Explanation,
            Points = 1.0m,
            CreatedAt = DateTime.UtcNow,
        }).ToList();

        await _repo.AddQuestionsAsync(questionEntities);
        assessment.Questions = questionEntities;

        return new {
            assessmentId = assessment.AssessmentId,
            title = assessment.Title,
            questions = assessment.Questions
        };
    }

    private static string ExtractTextFromPdf(string pdfPath)
    {
        var sb = new StringBuilder();
        using (var pdf = PdfDocument.Open(pdfPath))
        {
            foreach (Page page in pdf.GetPages())
            {
                sb.AppendLine(page.Text);
            }
        }
        var text = sb.ToString();
        if (text.Length > 8000)
        {
            text = text.Substring(0, 8000) + "...";
        }
        return text;
    }
}