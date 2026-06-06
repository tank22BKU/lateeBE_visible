using MediatR;

namespace PracticeSessionService.Application.Queries.GetPracticeSessionsByPatient;

public class GetPracticeSessionsByPatientRequest : IRequest<GetPracticeSessionsByPatientResponse>
{
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
}
