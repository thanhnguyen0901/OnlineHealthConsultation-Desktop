namespace OnlineHealthConsultation.Desktop.Models;

public sealed class AppointmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public PatientProfileDto? Patient { get; set; }

    public string PatientName => Patient?.User?.FullName ?? "Patient";
    public string ScheduledAtText => ScheduledAt?.LocalDateTime.ToString("dd/MM/yyyy HH:mm") ?? "-";
    public bool CanConfirm => Status.Equals("PENDING_CONFIRMATION", StringComparison.OrdinalIgnoreCase);
    public bool CanComplete => Status.Equals("CONFIRMED", StringComparison.OrdinalIgnoreCase);

    public string StatusText => Status.ToUpper() switch
    {
        "PENDING_CONFIRMATION" => "Chờ xác nhận",
        "CONFIRMED" => "Đã xác nhận",
        "COMPLETED" => "Đã hoàn thành",
        "CANCELLED" => "Đã hủy",
        _ => Status
    };
}
