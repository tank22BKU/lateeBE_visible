using MediatR;
using PracticeSessionService.Domain.Repositories;

namespace PracticeSessionService.Application.Queries.GetPracticeSessionsByPatient;

public class GetPracticeSessionsByPatientHandler
    : IRequestHandler<GetPracticeSessionsByPatientRequest, GetPracticeSessionsByPatientResponse>
{
    private readonly IPracticeSessionRepository _repo;

    public GetPracticeSessionsByPatientHandler(IPracticeSessionRepository repo)
    {
        _repo = repo;
    }

    public async Task<GetPracticeSessionsByPatientResponse> Handle(
        GetPracticeSessionsByPatientRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.LearnerId))
            throw new ArgumentException("learnerId is required.");
        if (string.IsNullOrWhiteSpace(request.PatientId))
            throw new ArgumentException("patientId is required.");

        var sessions = await _repo.GetSessionsByPatientAsync(request.LearnerId, request.PatientId);

        return new GetPracticeSessionsByPatientResponse
        {
            LearnerId = request.LearnerId,
            PatientId = request.PatientId,
            Items = sessions
                .Select(s => new PracticeSessionItemResponse
                {
                    SessionId = s.Id,
                    Status = s.Status,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    CreatedAt = s.CreatedAt,
                    FinalDiagnosis = s.FinalDiagnosis,
                })
                .ToList(),
        };
    }
}
