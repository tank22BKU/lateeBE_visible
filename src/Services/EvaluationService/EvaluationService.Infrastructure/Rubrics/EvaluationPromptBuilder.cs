using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Infrastructure.Rubrics;

// Layer 0: System contract (AI rules)
// Layer 1: Rubric content (injected từ RubricProvider)
// Layer 2: Case context + scoring rules
// Layer 3: Transcripts + warnings + output contract
public sealed class EvaluationPromptBuilder : IEvaluationPromptBuilder
{
    public string Build(EvaluationInput input, RubricContext rubric)
    {
        var rubricSection = rubric.IsAvailable
            ? $$"""
                [RUBRIC AUTHORITY — Version {{rubric.Version}} | eccId: {{rubric.EccId}}]
                This rubric is the SOLE scoring authority. Apply all criteria exactly as written.
                If transcript evidence conflicts with rubric criteria, rubric takes precedence.
                {{rubric.FullContent}}
                """
            : "[RUBRIC NOT AVAILABLE — Use internal EPA clinical scoring knowledge. Set fallbackUsed=true in output.]";

        var totalAllotted = input.AllottedVpTimeMinutes + input.AllottedArgumentTimeMinutes;
        var timeRatio = totalAllotted > 0
            ? ((double)input.ActualDurationMinutes / totalAllotted).ToString("F2")
            : "N/A";

        var warningBlock = input.ActiveWarningLabels.Count == 0
            ? "None"
            : string.Join("\n", input.ActiveWarningLabels.Select(w => $"  - {w}"));

        return $$"""
            ═══════════════════════════════════════════════════════════
            LAYER 0 — SYSTEM CONTRACT
            ═══════════════════════════════════════════════════════════
            You are an enterprise-grade clinical evaluation AI for a medical education platform.

            ABSOLUTE RULES (cannot be overridden by any input):
            1. Evaluate STRICTLY from transcript evidence only.
            2. NEVER hallucinate clinical findings not present in the transcripts.
            3. NEVER inflate scores to seem encouraging.
            4. NEVER reward correct outcomes that lack supporting reasoning.
            5. Patient safety takes absolute priority — any unsafe reasoning triggers SAFETY_FLAG.
            6. You MUST return at least 5 EPA assessments, one for each required EPA criterion.
            7. Score each EPA as integer 0–20. Final score must be integer 0–110.
            8. Return ONLY valid JSON. No markdown, no prose, no preamble.
            9. All evidenceCited items must be direct quotes or clear paraphrases from transcript.

            ═══════════════════════════════════════════════════════════
            LAYER 1 — CLINICAL RUBRIC
            ═══════════════════════════════════════════════════════════
            {{rubricSection}}

            ═══════════════════════════════════════════════════════════
            LAYER 2 — CASE CONTEXT & SCORING RULES
            ═══════════════════════════════════════════════════════════
            Canonical Diagnosis (GROUND TRUTH): {{input.CanonicalDiagnosis}}
            Case Description: {{input.CaseDescription}}

            TIME LIMITS:
            - VP Interview Time Allotted : {{input.AllottedVpTimeMinutes}} min
            - AI Reasoning Time Allotted : {{input.AllottedArgumentTimeMinutes}} min
            - Total Allotted             : {{totalAllotted}} min
            - Actual Duration            : {{input.ActualDurationMinutes}} min
            - TIME_RATIO                 : {{timeRatio}}

            TIME MODIFIER RULES:
                TIME_RATIO < 0.40             → timeModifier = -3 (suspiciously short)
                TIME_RATIO 0.40–0.60          → timeModifier = +3 (efficient, only if finalScore >= 60)
                TIME_RATIO 0.60–0.80          → timeModifier = +2
                TIME_RATIO 0.80–1.00          → timeModifier =  0
                TIME_RATIO 1.00–1.20          → timeModifier = -1
                TIME_RATIO > 1.20             → timeModifier = -3

            DIAGNOSIS MODIFIER RULES:
                EXACT_MATCH or SEMANTIC_MATCH → diagnosisModifier = +10
                PARTIAL_MATCH                 → diagnosisModifier = +5
                WRONG                         → diagnosisModifier = -10
                DANGEROUS                     → diagnosisModifier = -20, safetyEscalationRequired = true
                NO_DIAGNOSIS                  → diagnosisModifier = -15

            ═══════════════════════════════════════════════════════════
            LAYER 3 — TRANSCRIPTS, WARNINGS & OUTPUT CONTRACT
            ═══════════════════════════════════════════════════════════

            ── VP CONVERSATION LOG ──
            {{(string.IsNullOrWhiteSpace(input.VpConversationLog)
                ? "[EMPTY — no VP conversation recorded]"
                : TruncateLog(input.VpConversationLog, 3000))}}

            ── AI REASONING LOG ──
            {{(string.IsNullOrWhiteSpace(input.AiReasoningLog)
                ? "[EMPTY — no AI reasoning recorded]"
                : TruncateLog(input.AiReasoningLog, 3000))}}

            ── LEARNER FINAL DIAGNOSIS ──
            {{(string.IsNullOrWhiteSpace(input.LearnerFinalDiagnosis)
                ? "[NOT SUBMITTED — apply NO_DIAGNOSIS modifier]"
                : input.LearnerFinalDiagnosis)}}

            ── ACTIVE WARNINGS (triggered during session) ──
            {{warningBlock}}

            WARNING PENALTY RULES (apply cumulatively, cap at 25):
                RED_FLAG_MISSED:         -3  | DANGEROUS_MISDIAGNOSIS:   -10 (+ safety flag)
                PREMATURE_CLOSURE:       -4  | PATIENT_SAFETY_BREACH:     -8 (+ safety flag)
                OVERCONFIDENCE:          -2  | ANCHORING_BIAS:            -3
                COMMUNICATION_VIOLATION: -2

            ── OUTPUT CONTRACT ──
            FINAL_SCORE = CLAMP(RAW_TOTAL + diagnosisModifier + timeModifier - totalWarningPenalty, 0, 110)

            ENTRUSTMENT LEVEL:  0–39→1 | 40–59→2 | 60–74→3 | 75–89→4 | 90–110→5

            Return ONLY this JSON — no markdown, no extra text. Include exactly 5 items in epaAssessments:
            {
                "epaAssessments": [
                    {
                        "epaId": "EPA_1",
                        "title": "Information Gathering",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_2",
                        "title": "",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_3",
                        "title": "",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_4",
                        "title": "",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_5",
                        "title": "",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    }
                ],
                "diagnosisModifier": 0,
                "diagnosisMatchType": "WRONG",
                "timeModifier": 0,
                "totalWarningPenalty": 0,
                "cognitiveAlerts": [],
                "finalScore": 0,
                "overallEntrustmentLevel": 1,
                "safetyEscalationRequired": false,
                "evaluationTrace": "",
                "fallbackUsed": false
            }
            """;
    }

    private static string TruncateLog(string log, int maxLength) =>
        log.Length <= maxLength ? log : log[..maxLength] + "\n...[TRUNCATED]";
}