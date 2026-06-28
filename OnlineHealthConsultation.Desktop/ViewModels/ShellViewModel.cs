using Caliburn.Micro;
using OnlineHealthConsultation.Desktop.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
{
    private readonly IAuthSession _session;
    private readonly DoctorDashboardViewModel _dashboard;
    private readonly AppointmentsViewModel _appointments;
    private readonly QuestionsViewModel _questions;
    private readonly IWindowManager _windowManager;
    private readonly IServiceProvider _serviceProvider;

    public ShellViewModel(
        IAuthSession session,
        DoctorDashboardViewModel dashboard,
        AppointmentsViewModel appointments,
        QuestionsViewModel questions,
        IWindowManager windowManager,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _dashboard = dashboard;
        _appointments = appointments;
        _questions = questions;
        _windowManager = windowManager;
        _serviceProvider = serviceProvider;
        DisplayName = "Hệ thống Bác sĩ - Tư vấn Sức khỏe Trực tuyến";
        Items.AddRange([_dashboard, _appointments, _questions]);
    }

    public string DoctorName => _session.User?.FullName ?? "Bác sĩ";
    public string ApiBaseUrl => _session.ApiBaseUrl;

    public bool IsDashboardActive => ActiveItem == _dashboard;
    public bool IsAppointmentsActive => ActiveItem == _appointments;
    public bool IsQuestionsActive => ActiveItem == _questions;

    public override async Task ActivateItemAsync(IScreen item, CancellationToken cancellationToken = default)
    {
        await base.ActivateItemAsync(item, cancellationToken);
        NotifyOfPropertyChange(nameof(IsDashboardActive));
        NotifyOfPropertyChange(nameof(IsAppointmentsActive));
        NotifyOfPropertyChange(nameof(IsQuestionsActive));
    }

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await ActivateItemAsync(_dashboard, cancellationToken);
    }

    public Task ShowDashboard() => ActivateItemAsync(_dashboard);
    public Task ShowAppointments() => ActivateItemAsync(_appointments);
    public Task ShowQuestions() => ActivateItemAsync(_questions);

    public async Task Logout()
    {
        _session.SignOut();
        var loginVM = (LoginViewModel)_serviceProvider.GetService(typeof(LoginViewModel))!;
        await _windowManager.ShowWindowAsync(loginVM);
        await TryCloseAsync();
    }
}
