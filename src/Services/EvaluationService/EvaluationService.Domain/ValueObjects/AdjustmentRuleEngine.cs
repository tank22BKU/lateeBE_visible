using EvaluationService.Domain.Repositories;

namespace EvaluationService.Domain.ValueObjects;

/// <summary>
/// Tách hoàn toàn khỏi EPA scoring.
/// finalScore = CLAMP(pureEpaScore + adjustments.AdjustmentTotal, 0, 110)
///
/// WARNING LABELS (từ FE warnings[]) — KHÔNG liên quan đến validation categories của AI:
///   RED_FLAG_MISSED, DANGEROUS_MISDIAGNOSIS, PREMATURE_CLOSURE,
///   PATIENT_SAFETY_BREACH, OVERCONFIDENCE, ANCHORING_BIAS, COMMUNICATION_VIOLATION
///
/// VALIDATION CATEGORIES (AI classify từng turn trong transcript) — chỉ nằm trong
///   failurePatterns[] của EpaScore, KHÔNG đi qua engine này:
///   valid, ethics_violation, workflow_violation, unsafe_question,
///   irrelevant_question, clinical_reasoning_issue
/// </summary>
public static class AdjustmentRuleEngine
{
    public static ScoringAdjustments Calculate(
        string diagnosisMatchType,
        string learnerDiagnosis,
        string canonicalDiagnosis,
        int actualDurationMinutes,
        int allottedTotalMinutes,
        IReadOnlyList<string> warningLabels,
        IReadOnlyList<string> warningDescriptions,
        int pureEpaScore,
        AdjustmentExplanation? explanation = null
    )
    {
        var positive = new List<ScoringAdjustment>();
        var negative = new List<ScoringAdjustment>();

        var warningReasons =
            explanation
                ?.Warnings?.Where(w => !string.IsNullOrWhiteSpace(w.Label))
                .GroupBy(w => w.Label.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.Last().Reason ?? string.Empty)
            ?? new Dictionary<string, string>();

        // ── 1. Diagnosis modifier ────────────────────────────────────────
        ApplyDiagnosisAdjustment(
            diagnosisMatchType,
            learnerDiagnosis,
            canonicalDiagnosis,
            explanation?.Diagnosis,
            positive,
            negative
        );

        // ── 2. Time modifier ─────────────────────────────────────────────
        ApplyTimeAdjustment(
            actualDurationMinutes,
            allottedTotalMinutes,
            pureEpaScore,
            explanation?.Time,
            positive,
            negative
        );

        // ── 3. Warning penalties ─────────────────────────────────────────
        var (safetyEscalation, warningPenaltyTotal) = ApplyWarningAdjustments(
            warningLabels,
            warningDescriptions,
            warningReasons,
            negative
        );

        // Cap tổng warning penalty tại 25
        CapWarningPenalty(negative, warningPenaltyTotal);

        // ── 4. Validation summary ────────────────────────────────────────
        var validation = new ValidationSummary(
            HasEthicsViolation: warningLabels.Any(l =>
                l.Contains("ETHICS", StringComparison.OrdinalIgnoreCase)
            ),
            HasUnsafeQuestion: warningLabels.Any(l =>
                l.Contains("UNSAFE", StringComparison.OrdinalIgnoreCase)
                || l.Equals("PATIENT_SAFETY_BREACH", StringComparison.OrdinalIgnoreCase)
            ),
            HasWorkflowViolation: warningLabels.Any(l =>
                l.Contains("WORKFLOW", StringComparison.OrdinalIgnoreCase)
                || l.Equals("COMMUNICATION_VIOLATION", StringComparison.OrdinalIgnoreCase)
            ),
            SafetyEscalationRequired: safetyEscalation,
            TotalWarnings: warningLabels.Count
        );

        return new ScoringAdjustments(positive.AsReadOnly(), negative.AsReadOnly(), validation);
    }

    private static void ApplyDiagnosisAdjustment(
        string matchType,
        string learnerDx,
        string canonicalDx,
        string? aiReason,
        List<ScoringAdjustment> positive,
        List<ScoringAdjustment> negative
    )
    {
        var normalized = matchType.Trim().ToUpperInvariant();

        string DynamicReason(string template) =>
            string.IsNullOrWhiteSpace(learnerDx)
                ? template
                : $"{template} Learner submitted: \"{learnerDx}\". Canonical: \"{canonicalDx}\".";

        string Reason(string template) =>
            string.IsNullOrWhiteSpace(aiReason) ? DynamicReason(template) : aiReason;

        switch (normalized)
        {
            case "EXACT_MATCH":
                positive.Add(
                    new(
                        Code: "DIAGNOSIS_EXACT_MATCH",
                        Title: "Exact diagnosis match",
                        Score: +10,
                        Reason: Reason(
                            "Learner's diagnosis exactly matches the canonical diagnosis."
                        ),
                        Source: "diagnosis",
                        Severity: "positive"
                    )
                );
                break;

            case "SEMANTIC_MATCH":
                positive.Add(
                    new(
                        Code: "DIAGNOSIS_SEMANTIC_MATCH",
                        Title: "Semantic diagnosis match",
                        Score: +10,
                        Reason: Reason(
                            "Learner's diagnosis is clinically equivalent to the canonical diagnosis."
                        ),
                        Source: "diagnosis",
                        Severity: "positive"
                    )
                );
                break;

            case "PARTIAL_MATCH":
                positive.Add(
                    new(
                        Code: "DIAGNOSIS_PARTIAL_MATCH",
                        Title: "Partial diagnosis match",
                        Score: +5,
                        Reason: Reason(
                            "Learner identified the correct organ system or disease category but missed specifics."
                        ),
                        Source: "diagnosis",
                        Severity: "positive"
                    )
                );
                break;

            case "WRONG":
                negative.Add(
                    new(
                        Code: "DIAGNOSIS_WRONG",
                        Title: "Incorrect diagnosis",
                        Score: -10,
                        Reason: Reason(
                            "Learner's diagnosis does not match the canonical diagnosis and reflects a clinical reasoning error."
                        ),
                        Source: "diagnosis",
                        Severity: "high"
                    )
                );
                break;

            case "DANGEROUS":
                negative.Add(
                    new(
                        Code: "DIAGNOSIS_DANGEROUS",
                        Title: "Dangerous misdiagnosis",
                        Score: -20,
                        Reason: Reason(
                            "Learner's diagnosis is clinically dangerous and could cause patient harm if acted upon."
                        ),
                        Source: "diagnosis",
                        Severity: "critical"
                    )
                );
                break;

            case "NO_DIAGNOSIS":
                negative.Add(
                    new(
                        Code: "DIAGNOSIS_MISSING",
                        Title: "No diagnosis submitted",
                        Score: -15,
                        Reason: Reason(
                            "Learner did not submit a final diagnosis before ending the session."
                        ),
                        Source: "diagnosis",
                        Severity: "high"
                    )
                );
                break;

            // UNKNOWN / UNVERIFIED → no adjustment, no entry
        }
    }

    private static void ApplyTimeAdjustment(
        int actualMinutes,
        int allottedMinutes,
        int pureEpaScore,
        string? aiReason,
        List<ScoringAdjustment> positive,
        List<ScoringAdjustment> negative
    )
    {
        if (allottedMinutes <= 0)
            return;

        var ratio = (double)actualMinutes / allottedMinutes;

        string TimeReason(string context) =>
            string.IsNullOrWhiteSpace(aiReason)
                ? $"{context} Session used {actualMinutes} min out of {allottedMinutes} min allotted (ratio: {ratio:F2})."
                : aiReason;

        if (ratio < 0.40)
        {
            negative.Add(
                new(
                    Code: "TIME_TOO_SHORT",
                    Title: "Session suspiciously short",
                    Score: -3,
                    Reason: TimeReason(
                        "Session completed in less than 40% of allotted time, suggesting incomplete evaluation."
                    ),
                    Source: "time",
                    Severity: "medium"
                )
            );
        }
        else if (ratio < 0.60)
        {
            if (pureEpaScore >= 60)
                positive.Add(
                    new(
                        Code: "TIME_EFFICIENT",
                        Title: "High time efficiency",
                        Score: +3,
                        Reason: TimeReason(
                            "Learner completed the session efficiently with high clinical performance."
                        ),
                        Source: "time",
                        Severity: "positive"
                    )
                );
        }
        else if (ratio < 0.80)
        {
            positive.Add(
                new(
                    Code: "TIME_GOOD",
                    Title: "Good time management",
                    Score: +2,
                    Reason: TimeReason(
                        "Learner completed the session within a well-managed time frame."
                    ),
                    Source: "time",
                    Severity: "positive"
                )
            );
        }
        else if (ratio <= 1.00)
        {
            // On time
        }
        else if (ratio <= 1.20)
        {
            negative.Add(
                new(
                    Code: "TIME_OVER_SLIGHT",
                    Title: "Slightly over time",
                    Score: -1,
                    Reason: TimeReason("Session exceeded allotted time by up to 20%."),
                    Source: "time",
                    Severity: "low"
                )
            );
        }
        else
        {
            negative.Add(
                new(
                    Code: "TIME_OVER_SIGNIFICANT",
                    Title: "Significantly over time",
                    Score: -3,
                    Reason: TimeReason(
                        "Session exceeded allotted time by more than 20%, indicating poor time management."
                    ),
                    Source: "time",
                    Severity: "medium"
                )
            );
        }
    }

    private static (bool safetyEscalation, int totalPenalty) ApplyWarningAdjustments(
        IReadOnlyList<string> labels,
        IReadOnlyList<string> descriptions,
        IReadOnlyDictionary<string, string> aiReasons,
        List<ScoringAdjustment> negative
    )
    {
        bool safetyEscalation = false;
        int totalPenalty = 0;

        for (int i = 0; i < labels.Count; i++)
        {
            var label = labels[i];
            var description = i < descriptions.Count ? descriptions[i] : string.Empty;

            var normalized = label.ToUpperInvariant();
            var (penalty, isSafety, title, baseReason) = normalized switch
            {
                "RED_FLAG_MISSED" => (
                    3,
                    false,
                    "Red flag missed",
                    "Learner failed to identify or ask about a clinically significant red flag symptom."
                ),

                "DANGEROUS_MISDIAGNOSIS" => (
                    10,
                    true,
                    "Dangerous misdiagnosis warning",
                    "A potentially dangerous diagnostic error was flagged during the session."
                ),

                "PREMATURE_CLOSURE" => (
                    4,
                    false,
                    "Premature clinical closure",
                    "Learner stopped diagnostic reasoning before adequately exploring differential diagnoses."
                ),

                "PATIENT_SAFETY_BREACH" => (
                    8,
                    true,
                    "Patient safety breach",
                    "An action or recommendation that could directly compromise patient safety was detected."
                ),

                "OVERCONFIDENCE" => (
                    2,
                    false,
                    "Overconfidence bias",
                    "Learner expressed certainty disproportionate to the evidence available in the case."
                ),

                "ANCHORING_BIAS" => (
                    3,
                    false,
                    "Anchoring bias",
                    "Learner fixated on an initial hypothesis and failed to adequately reconsider when contradicting evidence emerged."
                ),

                "COMMUNICATION_VIOLATION" => (
                    2,
                    false,
                    "Communication violation",
                    "Learner used inappropriate, unsafe, or unprofessional language during the patient interaction."
                ),

                _ => (0, false, string.Empty, string.Empty),
            };

            if (penalty == 0)
                continue;

            if (isSafety)
                safetyEscalation = true;
            totalPenalty += penalty;

            var fullReason =
                aiReasons.TryGetValue(normalized, out var aiReason)
                && !string.IsNullOrWhiteSpace(aiReason)
                    ? aiReason
                : string.IsNullOrWhiteSpace(description) ? baseReason
                : $"{baseReason} Details: {description}";

            negative.Add(
                new(
                    Code: label.ToUpperInvariant(),
                    Title: title,
                    Score: -penalty,
                    Reason: fullReason,
                    Source: "warning",
                    Severity: isSafety ? "critical"
                        : penalty >= 4 ? "high"
                        : "medium"
                )
            );
        }

        return (safetyEscalation, totalPenalty);
    }

    private static void CapWarningPenalty(List<ScoringAdjustment> negative, int totalPenalty)
    {
        if (totalPenalty <= 25)
            return;

        var excess = totalPenalty - 25;
        var lastWarn = negative.FindLastIndex(a => a.Source == "warning");
        if (lastWarn < 0)
            return;

        var last = negative[lastWarn];
        negative[lastWarn] = last with
        {
            Score = last.Score + excess,
            Reason =
                last.Reason + $" (Warning penalty capped at 25 total; {excess} points reduced.)",
        };
    }

    // ────────────────────────────────────────────────────────────────────
    public static int ComputeFinalScore(int pureEpaScore, ScoringAdjustments adj) =>
        Math.Clamp(pureEpaScore + adj.AdjustmentTotal, 0, 110);

    public static int MapEntrustmentLevel(int finalScore) =>
        finalScore switch
        {
            <= 39 => 1,
            <= 59 => 2,
            <= 74 => 3,
            <= 89 => 4,
            _ => 5,
        };
}
