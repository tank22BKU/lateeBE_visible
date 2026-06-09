namespace VirtualPatientService.Application.Dtos;

public class AttemptSummaryDto
{
    public bool Attempted { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public decimal? BestScore { get; set; }
    public decimal? LatestScore { get; set; }
}
