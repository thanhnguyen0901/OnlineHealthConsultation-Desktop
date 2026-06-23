using Caliburn.Micro;
using OnlineHealthConsultation.Desktop.Models;
using OnlineHealthConsultation.Desktop.Services;

namespace OnlineHealthConsultation.Desktop.ViewModels;

public sealed class QuestionsViewModel : BaseScreen
{
    private readonly IApiClient _apiClient;
    private QuestionDto? _selectedQuestion;
    private string _answerContent = string.Empty;

    public QuestionsViewModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
        DisplayName = "Questions";
    }

    public BindableCollection<QuestionDto> Questions { get; } = [];

    public QuestionDto? SelectedQuestion
    {
        get => _selectedQuestion;
        set
        {
            _selectedQuestion = value;
            AnswerContent = string.Empty;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanAnswerSelected));
        }
    }

    public string AnswerContent
    {
        get => _answerContent;
        set
        {
            _answerContent = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanAnswerSelected));
        }
    }

    public bool CanAnswerSelected =>
        SelectedQuestion?.CanAnswer == true &&
        !IsBusy &&
        !string.IsNullOrWhiteSpace(AnswerContent);

    protected override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await Load();
    }

    public async Task Load()
    {
        await RunBusyAsync(async () =>
        {
            var items = await _apiClient.GetAssignedQuestionsAsync();
            Questions.Clear();
            Questions.AddRange(items.OrderByDescending(item => item.CreatedAt));
        });
    }

    public async Task AnswerSelected()
    {
        if (SelectedQuestion is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _apiClient.AnswerQuestionAsync(SelectedQuestion.Id, AnswerContent.Trim());
            AnswerContent = string.Empty;
            await Load();
        }, "Answer submitted.");
    }
}
