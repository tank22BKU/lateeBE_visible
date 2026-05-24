namespace ClinicalCaseService.Domain.Entities;

public class ClinicalCaseLab
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string? Label { get; set; }
    public string? Fluid { get; set; }
    public string? Category { get; set; }
    public string? Value { get; set; }
    public string? RangeLower { get; set; }
    public string? RangeUpper { get; set; }
}
