using MediatR;
using PracticeSessionService.Domain.Entities;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Commands.CreatePracticeSession;

public class CreatePracticeSessionHandler
    : IRequestHandler<CreatePracticeSessionCommand, CreatePracticeSessionResult>
{
    private readonly IPracticeSessionRepository _repository;

    public CreatePracticeSessionHandler(IPracticeSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreatePracticeSessionResult> Handle(
        CreatePracticeSessionCommand request,
        CancellationToken cancellationToken
    )
    {
        var finalSessionId = !string.IsNullOrEmpty(request.Id)
            ? request.Id
            : $"SESS_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var session = new PracticeSession
        {
            Id = finalSessionId,
            LearnerId = request.LearnerId!,
            PatientId = request.PatientId!,
            ModuleId = request.ModuleId ?? "EPA_STANDARD_V1",
            DiscussionType = request.DiscussionType ?? "Message Type",
            GuidelinesId = request.GuidelinesId,
            Status = request.Status ?? "Practicing",
            StartTime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.AddSessionAsync(session);

        return new CreatePracticeSessionResult { Id = finalSessionId };
    }
}
