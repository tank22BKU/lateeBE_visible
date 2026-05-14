namespace PracticeSessionService.Domain.Entities.Constants;

public static class WarningLabels
{
    public const string RedFlagMissed         = "RED_FLAG_MISSED";         // -3 pts
    public const string DangerousMisdiagnosis = "DANGEROUS_MISDIAGNOSIS";  // -10 pts + safety flag
    public const string PrematureClosure      = "PREMATURE_CLOSURE";       // -4 pts
    public const string PatientSafetyBreach   = "PATIENT_SAFETY_BREACH";   // -8 pts + safety flag
    public const string Overconfidence        = "OVERCONFIDENCE";          // -2 pts
    public const string AnchoringBias         = "ANCHORING_BIAS";          // -3 pts
    public const string CommunicationViolation= "COMMUNICATION_VIOLATION"; // -2 pts
}