using EvaluationService.Domain.Entities;

namespace EvaluationService.Domain.Services;

public interface IFeedbackPromptBuilder
{
    string Build(
        PracticeSession session,
        Evaluation evaluation,
        List<EvaluationEpaScore> epaScores,
        List<Warning> warnings,
        string canonicalDiagnosis,
        string caseDescription,
        int allottedVpTimeMinutes,
        int allottedArgumentTimeMinutes
    );
}
