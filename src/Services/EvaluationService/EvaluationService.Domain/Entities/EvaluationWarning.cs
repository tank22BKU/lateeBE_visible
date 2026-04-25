using System;

namespace EvaluationService.Domain.Entities;
public class EvaluationWarning
{
    public string WarningId { get; set; } = Guid.NewGuid().ToString("N");
    public string ResultId { get; set; } = null!;
    public string WarningType { get; set; } = null!; 
    public string WarningMessage { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EvaluationResult EvaluationResult { get; set; } = null!;

}