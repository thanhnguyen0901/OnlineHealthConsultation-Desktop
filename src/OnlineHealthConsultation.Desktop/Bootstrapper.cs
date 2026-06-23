using Caliburn.Micro;
using Microsoft.Extensions.DependencyInjection;
using OnlineHealthConsultation.Desktop.Services;
using OnlineHealthConsultation.Desktop.ViewModels;
using System.Windows;

namespace OnlineHealthConsultation.Desktop;

public sealed class Bootstrapper : BootstrapperBase
{
    private ServiceProvider _serviceProvider = null!;

    public Bootstrapper()
    {
        Initialize();
    }

    protected override void Configure()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IAuthSession, AuthSession>();
        services.AddSingleton<IApiClient, ApiClient>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DoctorDashboardViewModel>();
        services.AddTransient<AppointmentsViewModel>();
        services.AddTransient<QuestionsViewModel>();

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override object GetInstance(Type service, string key)
    {
        return _serviceProvider.GetService(service)
            ?? throw new InvalidOperationException($"Service not found: {service.FullName}");
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
        return _serviceProvider.GetServices(service).Cast<object>();
    }

    protected override void BuildUp(object instance)
    {
        // Constructor injection is used throughout this small client.
    }

    protected override async void OnStartup(object sender, StartupEventArgs e)
    {
        await DisplayRootViewForAsync<LoginViewModel>();
    }
}
