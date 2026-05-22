using EvaluationService.Domain.Entities;
using EvaluationService.Domain.Services;

namespace EvaluationService.Infrastructure.Rubrics;

public sealed class FeedbackPromptBuilder : IFeedbackPromptBuilder
{
    public string Build(
        PracticeSession session,
        Evaluation evaluation,
        List<EvaluationEpaScore> epaScores,
        List<Warning> warnings,
        string canonicalDiagnosis,
        string caseDescription,
        int allottedVpTimeMinutes,
        int allottedArgumentTimeMinutes
    )
    {
        var totalAllotted = allottedVpTimeMinutes + allottedArgumentTimeMinutes;
        var timeRatio =
            totalAllotted > 0
                ? ((double)(evaluation.Duration ?? 0) / totalAllotted).ToString("F2")
                : "N/A";

        var warningLabels = warnings
            .Select(w => string.IsNullOrWhiteSpace(w.Label) ? "UNKNOWN" : w.Label.Trim())
            .ToList();

        var warningBlock =
            warningLabels.Count == 0
                ? "None"
                : string.Join("\n", warningLabels.Select(w => $"  - {w}"));

        var epaBreakdown =
            epaScores.Count == 0
                ? "- No EPA breakdown available"
                : string.Join("\n\n", epaScores.Select(FormatEpaDetail));

        var cognitiveAlertsText =
            warnings.Count == 0
                ? "- None"
                : string.Join(
                    ", ",
                    warnings.Select(w =>
                        string.IsNullOrWhiteSpace(w.Label) ? "UNKNOWN" : w.Label.Trim()
                    )
                );

        return $$"""
			═══════════════════════════════════════════════════════════
			LAYER 0 — SYSTEM CONTRACT
			═══════════════════════════════════════════════════════════
			You are an enterprise-grade clinical feedback AI for a medical education platform.

			ABSOLUTE RULES:
			1. Evaluate STRICTLY from transcript evidence only.
			2. NEVER hallucinate clinical findings not in transcripts.
			3. NEVER inflate feedback to seem encouraging.
			4. NEVER reward correct outcomes without supporting reasoning.
			5. Patient safety is absolute priority — unsafe reasoning must be called out.
			6. Return ONLY valid JSON. No markdown, no prose, no preamble.
			7. Use the case context, transcripts, evaluation result, and warnings together.

			═══════════════════════════════════════════════════════════
			LAYER 1 — FEEDBACK TARGET
			═══════════════════════════════════════════════════════════
			Output only the JSON object below.
			Do not include markdown, explanations, code fences, or additional text.

			{
			"strength": "Write one coherent paragraph only. Describe 2-3 clinically meaningful strengths in continuous prose, without bullets, arrays, or separate objects. Each strength should include: (1) what the learner did well, (2) brief supporting evidence from the conversation or reasoning process, and (3) why it mattered clinically. Keep the tone concise but specific and realistic.",

			"weakness": "Write one coherent weakness section only. Group the main improvement areas by EPA using this exact format: EPA 1: [EPA name] — [missing or weak clinical behaviors, reasoning gaps, communication issues, or unsafe actions]. EPA 2: [EPA name] — [missing or weak clinical behaviors, reasoning gaps, communication issues, or unsafe actions]. End with exactly one concluding sentence that summarizes the overall improvement direction. Focus only on meaningful weaknesses that affected diagnostic quality, patient safety, clinical reasoning, information gathering, or professionalism.",

			"overallAttemptFeedback": "Write a balanced coaching-style summary in 1-2 short paragraphs. Summarize the learner’s overall clinical reasoning, communication, diagnostic approach, organization, and patient safety performance. Highlight both strengths and the most important areas needing improvement. The tone should be constructive, professional, and supportive like feedback from a real clinical instructor.",

			"overallLabel": "Choose exactly ONE label based on the learner’s overall clinical performance: EXCELLENT, GOOD, DEVELOPING, or NEEDS_IMPROVEMENT."
			}


			═══════════════════════════════════════════════════════════
			LAYER 2 — CASE CONTEXT
			═══════════════════════════════════════════════════════════
			Canonical Diagnosis (GROUND TRUTH): {{canonicalDiagnosis}}
			Case Description: {{caseDescription}}

			Session timing (informational — do NOT factor into feedback tone):
			- VP Interview allotted : {{allottedVpTimeMinutes}} min
			- AI Reasoning allotted : {{allottedArgumentTimeMinutes}} min
			- Total allotted        : {{totalAllotted}} min
			- Actual duration       : {{evaluation.Duration ?? 0}} min
			- Time ratio            : {{timeRatio}}

			Warnings triggered (informational — use as evidence, do NOT invent new ones):
			{{warningBlock}}

			═══════════════════════════════════════════════════════════
			LAYER 3 — TRANSCRIPTS
			═══════════════════════════════════════════════════════════
			── VP CONVERSATION LOG ──
			{{(string.IsNullOrWhiteSpace(session.VpConversationLog)
				? "[EMPTY — no VP conversation recorded]"
				: Truncate(session.VpConversationLog, 3000))}}

			── AI REASONING LOG ──
			{{(string.IsNullOrWhiteSpace(session.AiReasoningLog)
				? "[EMPTY — no AI reasoning recorded]"
				: Truncate(session.AiReasoningLog, 3000))}}

			── LEARNER FINAL DIAGNOSIS ──
			{{(string.IsNullOrWhiteSpace(session.FinalDiagnosis)
				? "[NOT SUBMITTED]"
				: session.FinalDiagnosis)}}

			═══════════════════════════════════════════════════════════
			LAYER 4 — VALIDATION CATEGORIES
			═══════════════════════════════════════════════════════════
			For each observed learner action or question, classify the behavior in your reasoning as one of:
				"valid"                   — clinically appropriate action
				"ethics_violation"        — ethical breach detected
				"workflow_violation"      — deviated from expected clinical workflow
				"unsafe_question"         — question that could endanger patient safety
				"irrelevant_question"     — question unrelated to the clinical case
				"clinical_reasoning_issue"— flawed clinical reasoning detected

			═══════════════════════════════════════════════════════════
			LAYER 5 — EVALUATION RESULT
			═══════════════════════════════════════════════════════════
			- Evaluation ID: {{evaluation.Id}}
			- Score: {{(int)(evaluation.Score ?? 0)}}/110
			- Pure EPA score: {{evaluation.PureEpaScore}}/100
			- Entrustment level: {{evaluation.EntrustmentLevel ?? 1}}/5
			- Rubric version: {{(
				string.IsNullOrWhiteSpace(evaluation.RubricVersion)
					? "Unknown"
					: evaluation.RubricVersion
			)}}
			- Safety escalation required: False
			- Cognitive alerts: {{cognitiveAlertsText}}
			- Evaluation trace summary: {{Truncate(evaluation.FeedbackDetail, 1000)}}

			EPA BREAKDOWN (FULL INPUT):
			{{epaBreakdown}}

			FEEDBACK GUIDANCE:
			- strength: return one single paragraph only. Do not use bullets, arrays, or object-style formatting; keep all strengths in continuous prose.
			- weakness: return exactly one combined weakness section only. Use only the EPAs already present in EPA BREAKDOWN, write detailed improvement points EPA-by-EPA in plain text, and finish with one concise concluding sentence that summarizes the overall improvement direction.
			- overallAttemptFeedback: summarize performance in plain clinical language; include whether the learner was safe, efficient, and reasoning clearly.
			- overallLabel: choose the label that best matches the session, not the most encouraging label.
		""";
    }

    private static string FormatEpaDetail(EvaluationEpaScore epa)
    {
        var evidence = FormatList(epa.EvidenceCited);
        var failures = FormatList(epa.FailurePatterns);
        var safetyFlags = FormatList(epa.SafetyFlags);

        return $$"""
                - EPA ID: {{epa.EpaId}}
                Numerical Score: {{epa.NumericalScore}}/20
                Entrustment Level: {{epa.EntrustmentLevel}}
                Feedback Detail: {{Truncate(epa.FeedbackDetail, 800)}}
                Evidence Cited: {{evidence}}
                Failure Patterns: {{failures}}
                Safety Flags: {{safetyFlags}}
            """;
    }

    private static string FormatList(IEnumerable<string> values)
    {
        var list = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return list.Count == 0 ? "none" : string.Join("; ", list);
    }

    private static string Truncate(string? text, int max) =>
        string.IsNullOrWhiteSpace(text) ? "(empty)"
        : text.Length <= max ? text
        : text.Substring(0, Math.Min(text.Length, max)) + "...[truncated]";
}
