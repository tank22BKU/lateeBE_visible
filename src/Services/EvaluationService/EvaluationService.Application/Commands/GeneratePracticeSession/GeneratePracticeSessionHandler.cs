using MediatR;
using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Repositories;

namespace EvaluationService.Application.Commands.GeneratePracticeSession;

public class GeneratePracticeSessionHandler : IRequestHandler<GeneratePracticeSessionCommand, GeneratePracticeSessionResult>
{
    private readonly IEvaluationRepository _repository;

    public GeneratePracticeSessionHandler(IEvaluationRepository repository)
    {
        _repository = repository;
    }

    public async Task<GeneratePracticeSessionResult> Handle(GeneratePracticeSessionCommand request, CancellationToken cancellationToken)
    {
        var finalSessionId = !string.IsNullOrEmpty(request.Id)
            ? request.Id
            : $"SESS_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var session = new PracticeSession
        {
            Id = finalSessionId,
            LearnerId = request.LearnerId!,
            ClinicalCaseId = request.ClinicalCaseId!,
            Status = request.Status ?? "Practicing",
            StartTime = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.AddPracticeSessionAsync(session);

        return new GeneratePracticeSessionResult { Id = finalSessionId };
    }
}
