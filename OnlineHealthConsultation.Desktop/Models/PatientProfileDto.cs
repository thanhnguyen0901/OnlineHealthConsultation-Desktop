namespace OnlineHealthConsultation.Desktop.Models;

public sealed class PatientProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public UserDto? User { get; set; }
}
