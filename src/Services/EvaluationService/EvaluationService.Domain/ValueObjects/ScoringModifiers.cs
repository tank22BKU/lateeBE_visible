namespace EvaluationService.Domain.ValueObjects;

// Input: raw EPA scores, warnings, time, diagnosis match type
// Output: final score, entrustment level, breakdown
// CÔNG THỨC TỔNG:
// FINAL = CLAMP(RAW_TOTAL + DiagnosisMod + TimeMod - WarningPenalty, 0, 110)

// Gọi ScoringModifiers.Calculate() để get full score

public sealed class ScoringModifiers
{
    public int RawTotal { get; private init; }
    public int DiagnosisModifier { get; private init; }
    public int TimeModifier { get; private init; }
    public int WarningPenalty { get; private init; }
    public int FinalScore { get; private init; }
    public int EntrustmentLevel { get; private init; }
    public bool SafetyEscalationRequired { get; private init; }

    private ScoringModifiers() { }

    //   var scoring = ScoringModifiers.Calculate(
    //       rawTotal: 58,
    //       diagnosisMatchType: "SEMANTIC_MATCH",
    //       actualDuration: 38,
    //       allottedTotal: 45,
    //       warningLabels: ["RED_FLAG_MISSED", "PREMATURE_CLOSURE"]
    //   );
    
    public static ScoringModifiers Calculate(
        int rawTotal,
        string diagnosisMatchType,
        int actualDurationMinutes,
        int allottedTotalMinutes,
        IReadOnlyList<string> warningLabels)
    {
        var diagMod = CalculateDiagnosisModifier(diagnosisMatchType);
        var timeMod = CalculateTimeModifier(actualDurationMinutes, allottedTotalMinutes);
        var (warningPenalty, safetyEscalation) = CalculateWarningPenalty(warningLabels);

        var finalScore = Math.Clamp(
            rawTotal + diagMod + timeMod - warningPenalty,
            0, 110);

        return new ScoringModifiers
        {
            RawTotal = rawTotal,
            DiagnosisModifier = diagMod,
            TimeModifier = timeMod,
            WarningPenalty = warningPenalty,
            FinalScore = finalScore,
            EntrustmentLevel = MapEntrustmentLevel(finalScore),
            SafetyEscalationRequired = safetyEscalation
        };
    }

    private static int CalculateDiagnosisModifier(string matchType) =>
        matchType.ToUpperInvariant() switch
        {
            "EXACT_MATCH"      => 10,
            "SEMANTIC_MATCH"   => 10,
            "PARTIAL_MATCH"    => 5,
            "WRONG"            => -10,
            "DANGEROUS"        => -20,
            "NO_DIAGNOSIS"     => -15,
            _                  => 0     // UNKNOWN / UNVERIFIED từ fallback
        };

    private static int CalculateTimeModifier(int actualMinutes, int allottedMinutes)
    {
        if (allottedMinutes <= 0) return 0;

        var ratio = (double)actualMinutes / allottedMinutes;

        return ratio switch
        {
            < 0.40            => -3,   // Quá nhanh
            < 0.60            => +3,   // Hiệu quả cao (guard: chỉ cộng nếu score >= 60)
            < 0.80            => +2,   // Tốt
            <= 1.00           => 0,    // Đúng giờ
            <= 1.20           => -1,   // Hơi chậm
            _                 => -3    // Quá chậm
        };
    }


    /// Max tại 25 points.
    /// Safety escalation nếu có DANGEROUS hoặc SAFETY warning.
    private static (int Penalty, bool SafetyEscalation) CalculateWarningPenalty(
        IReadOnlyList<string> labels)
    {
        var total = 0;
        var escalation = false;

        foreach (var label in labels)
        {
            var (penalty, isHardSafety) = label.ToUpperInvariant() switch
            {
                "RED_FLAG_MISSED"          => (3,  false),
                "DANGEROUS_MISDIAGNOSIS"   => (10, true),
                "PREMATURE_CLOSURE"        => (4,  false),
                "PATIENT_SAFETY_BREACH"    => (8,  true),
                "OVERCONFIDENCE"           => (2,  false),
                "ANCHORING_BIAS"           => (3,  false),
                "COMMUNICATION_VIOLATION"  => (2,  false),
                _                          => (0,  false)
            };

            total += penalty;
            if (isHardSafety) escalation = true;
        }

        return (Math.Min(total, 25), escalation);
    }

    /// Rubric Section 3.4 — Map Final Score → Entrustment Level.
    /// 0–39 → L1, 40–59 → L2, 60–74 → L3, 75–89 → L4, 90–110 → L5
    public static int MapEntrustmentLevel(int finalScore) =>
        finalScore switch
        {
            <= 39  => 1,
            <= 59  => 2,
            <= 74  => 3,
            <= 89  => 4,
            _      => 5
        };
}