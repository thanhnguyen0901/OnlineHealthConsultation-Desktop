using Caliburn.Micro;
using OnlineHealthConsultation.Desktop.Services;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public sealed class LoginViewModel : BaseScreen
{
    private readonly IApiClient _apiClient;
    private readonly IAuthSession _session;
    private readonly IWindowManager _windowManager;
    private readonly IServiceProvider _serviceProvider;
    private string _email = "doctor.e2e@healthcare.local";
    private string _password = "Doctor@123";

    public LoginViewModel(
        IApiClient apiClient,
        IAuthSession session,
        IWindowManager windowManager,
        IServiceProvider serviceProvider)
    {
        _apiClient = apiClient;
        _session = session;
        _windowManager = windowManager;
        _serviceProvider = serviceProvider;
        DisplayName = "Đăng nhập Bác sĩ";
    }

    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanLogin));
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanLogin));
        }
    }

    public bool CanLogin =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password);

    public async Task Login()
    {
        await RunBusyAsync(async () =>
        {
            var result = await _apiClient.LoginAsync(Email.Trim(), Password);
            var user = result.User!;

            if (!user.Role.Equals("DOCTOR", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Ứng dụng desktop chỉ dành cho tài khoản Bác sĩ.");
            }

            _session.SignIn(result.AccessToken, user);
            var shell = (ShellViewModel)_serviceProvider.GetService(typeof(ShellViewModel))!;
            await _windowManager.ShowWindowAsync(shell);
            await TryCloseAsync();
        });
    }
}
