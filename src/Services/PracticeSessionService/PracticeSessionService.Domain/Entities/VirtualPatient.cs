namespace PracticeSessionService.Domain.Entities;

public class VirtualPatient
{
    public string PatientId { get; set; } = null!;  
    public char Gender { get; set; }               
    public int Age { get; set; }                   
    public string? Behaviors { get; set; }         
    public string? Descriptions { get; set; }       
    public DateTime CreatedAt { get; set; }         
    public DateTime UpdatedAt { get; set; }         
}
