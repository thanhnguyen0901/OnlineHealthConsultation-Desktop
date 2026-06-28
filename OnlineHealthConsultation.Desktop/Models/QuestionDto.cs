namespace OnlineHealthConsultation.Desktop.Models;

public sealed class QuestionDto
{
    public string Id { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public PatientProfileDto? Patient { get; set; }
    public List<AnswerDto> Answers { get; set; } = [];

    public string PatientName => Patient?.User?.FullName ?? "Patient";
    public string CreatedAtText => CreatedAt?.LocalDateTime.ToString("dd/MM/yyyy HH:mm") ?? "-";
    public bool CanAnswer => Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase);

    public string StatusText => Status.ToUpper() switch
    {
        "PENDING" => "Chờ trả lời",
        "ANSWERED" => "Đã trả lời",
        _ => Status
    };
}

public sealed class AnswerDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
}
