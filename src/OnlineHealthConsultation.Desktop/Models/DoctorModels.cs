namespace OnlineHealthConsultation.Desktop.Models;

public sealed class DoctorProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public int? YearsOfExperience { get; set; }
    public decimal? AvgRating { get; set; }
    public int? RatingCount { get; set; }
    public UserDto? User { get; set; }
    public List<DoctorSpecialtyLinkDto> Specialties { get; set; } = [];
}

public sealed class DoctorSpecialtyLinkDto
{
    public SpecialtyDto? Specialty { get; set; }
}

public sealed class SpecialtyDto
{
    public string Id { get; set; } = string.Empty;
    public string NameVi { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}
