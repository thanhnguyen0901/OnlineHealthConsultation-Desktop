using Caliburn.Micro;
using OnlineHealthConsultation.Desktop.Models;
using OnlineHealthConsultation.Desktop.Services;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public sealed class AppointmentsViewModel : BaseScreen
{
    private readonly IApiClient _apiClient;
    private AppointmentDto? _selectedAppointment;
    private string? _statusFilter;

    public AppointmentsViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        DisplayName = "Appointments";
    }

    public BindableCollection<string> StatusFilters { get; } = ["All", "Pending", "Confirmed", "Completed", "Cancelled"];
    public BindableCollection<AppointmentDto> Appointments { get; } = [];

    public AppointmentDto? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            _selectedAppointment = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanConfirmSelected));
            NotifyOfPropertyChange(nameof(CanCompleteSelected));
        }
    }

    public string? StatusFilter
    {
        get => _statusFilter;
        set
        {
            _statusFilter = value;
            NotifyOfPropertyChange();
        }
    }

    public bool CanConfirmSelected => SelectedAppointment?.CanConfirm == true && !IsBusy;
    public bool CanCompleteSelected => SelectedAppointment?.CanComplete == true && !IsBusy;

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await Load();
    }

    public async Task Load()
    {
        await RunBusyAsync(async () =>
        {
            var backendStatus = StatusFilter switch
            {
                "Pending" => "PENDING_CONFIRMATION",
                "Confirmed" => "CONFIRMED",
                "Completed" => "COMPLETED",
                "Cancelled" => "CANCELLED",
                _ => null
            };
            var items = await _apiClient.GetDoctorAppointmentsAsync(backendStatus);
            Appointments.Clear();
            Appointments.AddRange(items.OrderBy(item => item.ScheduledAt));
        });
    }

    public async Task ConfirmSelected()
    {
        if (SelectedAppointment is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _apiClient.ConfirmAppointmentAsync(SelectedAppointment.Id);
            await Load();
        }, "Appointment confirmed.");
    }

    public async Task CompleteSelected()
    {
        if (SelectedAppointment is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _apiClient.CompleteAppointmentAsync(SelectedAppointment.Id);
            await Load();
        }, "Appointment completed.");
    }
}
