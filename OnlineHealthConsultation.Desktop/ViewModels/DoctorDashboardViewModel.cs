using OnlineHealthConsultation.Desktop.Models;
using OnlineHealthConsultation.Desktop.Services;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public sealed class DoctorDashboardViewModel : BaseScreen
{
    private readonly IApiClient _apiClient;
    private DoctorProfileDto? _profile;
    private int _appointmentCount;
    private int _pendingQuestionCount;

    public DoctorDashboardViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        DisplayName = "Dashboard";
    }

    public DoctorProfileDto? Profile
    {
        get => _profile;
        set
        {
            _profile = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(DoctorName));
            NotifyOfPropertyChange(nameof(SpecialtyText));
        }
    }

    public string DoctorName => Profile?.User?.FullName ?? "Doctor";
    public string SpecialtyText => Profile?.Specialties.FirstOrDefault()?.Specialty?.NameVi ?? "Chua cap nhat";

    public int AppointmentCount
    {
        get => _appointmentCount;
        set
        {
            _appointmentCount = value;
            NotifyOfPropertyChange();
        }
    }

    public int PendingQuestionCount
    {
        get => _pendingQuestionCount;
        set
        {
            _pendingQuestionCount = value;
            NotifyOfPropertyChange();
        }
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await Load();
    }

    public async Task Load()
    {
        await RunBusyAsync(async () =>
        {
            Profile = await _apiClient.GetDoctorProfileAsync();
            var appointments = await _apiClient.GetDoctorAppointmentsAsync();
            var questions = await _apiClient.GetAssignedQuestionsAsync();
            AppointmentCount = appointments.Count;
            PendingQuestionCount = questions.Count(item => item.CanAnswer);
        });
    }
}
