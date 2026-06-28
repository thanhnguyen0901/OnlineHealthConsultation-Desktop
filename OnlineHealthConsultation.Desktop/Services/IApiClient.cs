using OnlineHealthConsultation.Desktop.Models;

namespace OnlineHealthConsultation.Desktop.Services;

public interface IApiClient
{
    Task<AuthResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<DoctorProfileDto> GetDoctorProfileAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppointmentDto>> GetDoctorAppointmentsAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<AppointmentDto> ConfirmAppointmentAsync(string appointmentId, CancellationToken cancellationToken = default);
    Task<AppointmentDto> CompleteAppointmentAsync(string appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuestionDto>> GetAssignedQuestionsAsync(CancellationToken cancellationToken = default);
    Task AnswerQuestionAsync(string questionId, string content, CancellationToken cancellationToken = default);
}
