using Caliburn.Micro;
using OnlineHealthConsultation.Desktop.Services;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly IAuthSession _session;
    private readonly DoctorDashboardViewModel _dashboard;
    private readonly AppointmentsViewModel _appointments;
    private readonly QuestionsViewModel _questions;

    public ShellViewModel(
        IAuthSession session,
        DoctorDashboardViewModel dashboard,
        AppointmentsViewModel appointments,
        QuestionsViewModel questions)
    {
        _session = session;
        _dashboard = dashboard;
        _appointments = appointments;
        _questions = questions;
        DisplayName = "Online Health Consultation - Doctor Workstation";
        Items.AddRange([_dashboard, _appointments, _questions]);
    }

    public string DoctorName => _session.User?.FullName ?? "Doctor";
    public string ApiBaseUrl => _session.ApiBaseUrl;

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await ActivateItemAsync(_dashboard, cancellationToken);
    }

    public Task ShowDashboard() => ActivateItemAsync(_dashboard);
    public Task ShowAppointments() => ActivateItemAsync(_appointments);
    public Task ShowQuestions() => ActivateItemAsync(_questions);
}
