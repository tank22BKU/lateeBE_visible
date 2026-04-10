namespace RoadmapService.Domain.Entities;

public class VirtualPatient
{
    public string PatientId { get; set; } = null!;  // VARCHAR(10) PRIMARY KEY
    public char Gender { get; set; }                // CHAR(1) CHECK (gender IN ('M','F'))
    public int Age { get; set; }                    // INT
    public string? Behaviors { get; set; }          // TEXT
    public string? Description { get; set; }        // TEXT
    public DateTime CreatedAt { get; set; }         // TIMESTAMP
    public DateTime UpdatedAt { get; set; }         // TIMESTAMP
}
