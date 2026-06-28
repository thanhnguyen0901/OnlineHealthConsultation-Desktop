using Caliburn.Micro;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public abstract class BaseScreen : Screen
{
    private bool _isBusy;
    private string? _statusMessage;
    private string? _errorMessage;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            NotifyOfPropertyChange();
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            NotifyOfPropertyChange();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected async Task RunBusyAsync(Func<Task> action, string? successMessage = null)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await action();
            StatusMessage = successMessage;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
