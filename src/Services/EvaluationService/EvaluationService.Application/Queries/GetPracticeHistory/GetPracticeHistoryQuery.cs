using MediatR;

namespace EvaluationService.Application.Queries.GetPracticeHistory;

public class GetPracticeHistoryQuery : IRequest<PracticeHistoryResponse>
{
    public string LearnerId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
}
