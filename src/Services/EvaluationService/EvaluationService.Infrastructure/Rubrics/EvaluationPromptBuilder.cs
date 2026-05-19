using EvaluationService.Domain.Repositories;
using EvaluationService.Domain.Services;
using EvaluationService.Domain.ValueObjects;

namespace EvaluationService.Infrastructure.Rubrics;

public sealed class EvaluationPromptBuilder : IEvaluationPromptBuilder
{
    public string Build(EvaluationInput input, RubricContext rubric)
    {
        var rubricSection = rubric.IsAvailable
            ? $"""
                [RUBRIC AUTHORITY — Version {rubric.Version} | eccId: {rubric.EccId}]
                This rubric is the SOLE scoring authority. Apply all criteria exactly as written.
                {rubric.FullContent}
                """
            : "[RUBRIC NOT AVAILABLE — Use internal EPA clinical scoring knowledge.]";

        var totalAllotted = input.AllottedVpTimeMinutes + input.AllottedArgumentTimeMinutes;
        var timeRatio =
            totalAllotted > 0
                ? ((double)input.ActualDurationMinutes / totalAllotted).ToString("F2")
                : "N/A";

        var warningBlock =
            input.ActiveWarningLabels.Count == 0
                ? "None"
                : string.Join("\n", input.ActiveWarningLabels.Select(w => $"  - {w}"));

        return $$"""
            ═══════════════════════════════════════════════════════════
            LAYER 0 — SYSTEM CONTRACT
            ═══════════════════════════════════════════════════════════
            You are an enterprise-grade clinical evaluation AI for a medical education platform.

            ABSOLUTE RULES:
            1. Evaluate STRICTLY from transcript evidence only.
            2. NEVER hallucinate clinical findings not in transcripts.
            3. NEVER inflate scores to seem encouraging.
            4. NEVER reward correct outcomes without supporting reasoning.
            5. Patient safety is absolute priority — unsafe reasoning → SAFETY_FLAG.
            6. Return EXACTLY 5 EPA assessments.
            7. Score each EPA as integer 0–20. Your finalScore field is informational only.
            8. Return ONLY valid JSON. No markdown, no prose, no preamble.
            9. evidenceCited = direct quotes or clear paraphrases from transcript.
            10. YOUR TASK: Score pure clinical performance ONLY.
                Diagnosis modifiers, time modifiers, warning penalties are computed by backend.
                Do NOT apply them to your EPA scores.
            11. Provide adjustmentExplanations with short, single-sentence reasons.
                Max 160 chars each. No calculations, ratios, or quoted diagnoses.

            ═══════════════════════════════════════════════════════════
            LAYER 1 — CLINICAL RUBRIC
            ═══════════════════════════════════════════════════════════
            {{rubricSection}}

            ═══════════════════════════════════════════════════════════
            LAYER 2 — CASE CONTEXT
            ═══════════════════════════════════════════════════════════
            Canonical Diagnosis (GROUND TRUTH): {{input.CanonicalDiagnosis}}
            Case Description: {{input.CaseDescription}}

            Session timing (informational — do NOT factor into EPA scores):
            - VP Interview allotted : {{input.AllottedVpTimeMinutes}} min
            - AI Reasoning allotted : {{input.AllottedArgumentTimeMinutes}} min
            - Total allotted        : {{totalAllotted}} min
            - Actual duration       : {{input.ActualDurationMinutes}} min
            - Time ratio            : {{timeRatio}}

            Warnings triggered (informational — do NOT factor into EPA scores):
            {{warningBlock}}

            ═══════════════════════════════════════════════════════════
            LAYER 3 — TRANSCRIPTS
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
                ? "[NOT SUBMITTED]"
                : input.LearnerFinalDiagnosis)}}

            ═══════════════════════════════════════════════════════════
            LAYER 4 — VALIDATION CATEGORIES
            ═══════════════════════════════════════════════════════════
            For each EPA, classify each observed learner action or question into failurePatterns[].
            Each entry in failurePatterns must be exactly one of:
                "valid"                   — clinically appropriate action
                "ethics_violation"        — ethical breach detected
                "workflow_violation"      — deviated from expected clinical workflow
                "unsafe_question"         — question that could endanger patient safety
                "irrelevant_question"     — question unrelated to the clinical case
                "clinical_reasoning_issue"— flawed clinical reasoning detected

            ═══════════════════════════════════════════════════════════
            LAYER 5 — DIAGNOSIS CLASSIFICATION
            ═══════════════════════════════════════════════════════════
            Compare learner's final diagnosis to canonical diagnosis.
            diagnosisMatchType must be exactly one of:
                EXACT_MATCH    — identical or medically equivalent
                SEMANTIC_MATCH — clinically equivalent (different terminology)
                PARTIAL_MATCH  — correct organ system, wrong specifics
                WRONG          — incorrect diagnosis
                DANGEROUS      — diagnosis that would cause patient harm
                NO_DIAGNOSIS   — learner did not submit a diagnosis

            ═══════════════════════════════════════════════════════════
            LAYER 6 — OUTPUT CONTRACT
            ═══════════════════════════════════════════════════════════
            ENTRUSTMENT LEVEL per EPA: 0–3→1 | 4–7→2 | 8–11→3 | 12–15→4 | 16–20→5
            OVERALL: 0–39→1 | 40–59→2 | 60–74→3 | 75–89→4 | 90–100→5

            Return ONLY valid JSON, no markdown, no extra text:
            {
                "epaAssessments": [
                    {
                        "epaId": "EPA_1",
                        "title": "Information Gathering",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "<required: explain WHY this score — cite specific transcript evidence>",
                        "evidenceCited": ["<direct quote or paraphrase from transcript>"],
                        "failurePatterns": ["<valid|ethics_violation|workflow_violation|unsafe_question|irrelevant_question|clinical_reasoning_issue>"],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_2",
                        "title": "Differential Diagnosis",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "<required: explain WHY this score>",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_3",
                        "title": "Clinical Reasoning",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "<required: explain WHY this score>",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_4",
                        "title": "Critical Thinking",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "<required: explain WHY this score>",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    },
                    {
                        "epaId": "EPA_5",
                        "title": "Efficiency and Professionalism",
                        "score": 0,
                        "entrustmentLevel": 1,
                        "feedback": "<required: explain WHY this score>",
                        "evidenceCited": [],
                        "failurePatterns": [],
                        "safetyFlags": []
                    }
                ],
                "diagnosisMatchType": "WRONG",
                "cognitiveAlerts": [],
                "finalScore": 0,
                "overallEntrustmentLevel": 1,
                "safetyEscalationRequired": false,
                "evaluationTrace": "<brief summary of overall performance and key observations>",
                "adjustmentExplanations": {
                    "diagnosis": "<short reason for diagnosis adjustment>",
                    "time": "<short reason for time adjustment>",
                    "warnings": [
                        {
                            "label": "RED_FLAG_MISSED",
                            "reason": "<short reason for warning penalty>"
                        }
                    ]
                },
                "diagnosisModifier": 0,
                "timeModifier": 0,
                "totalWarningPenalty": 0,
                "fallbackUsed": false
            }
            """;
    }

    private static string TruncateLog(string log, int maxLength) =>
        log.Length <= maxLength ? log : log[..maxLength] + "\n...[TRUNCATED]";
}
