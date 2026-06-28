using OnlineHealthConsultation.Desktop.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OnlineHealthConsultation.Desktop.Services;

public sealed class ApiClient : IApiClient
{
    private readonly IAuthSession _session;
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiClient(IAuthSession session)
    {
        _session = session;
    }

    public async Task<AuthResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<AuthResultDto>(
            HttpMethod.Post,
            "auth/login",
            new { email, password },
            requiresAuth: false,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(response.AccessToken) || response.User is null)
        {
            throw new InvalidOperationException("Login response does not include an access token or user.");
        }

        return response;
    }

    public Task<DoctorProfileDto> GetDoctorProfileAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync<DoctorProfileDto>(HttpMethod.Get, "doctors/me/profile", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetDoctorAppointmentsAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(status)
            ? "appointments/doctor/me"
            : $"appointments/doctor/me?status={Uri.EscapeDataString(status)}";

        return await SendAsync<List<AppointmentDto>>(HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    public Task<AppointmentDto> ConfirmAppointmentAsync(string appointmentId, CancellationToken cancellationToken = default)
    {
        return SendAsync<AppointmentDto>(HttpMethod.Patch, $"appointments/{appointmentId}/confirm", cancellationToken: cancellationToken);
    }

    public Task<AppointmentDto> CompleteAppointmentAsync(string appointmentId, CancellationToken cancellationToken = default)
    {
        return SendAsync<AppointmentDto>(HttpMethod.Patch, $"appointments/{appointmentId}/complete", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<QuestionDto>> GetAssignedQuestionsAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<QuestionDto>>(HttpMethod.Get, "questions/assigned", cancellationToken: cancellationToken);
    }

    public async Task AnswerQuestionAsync(string questionId, string content, CancellationToken cancellationToken = default)
    {
        await SendAsync<JsonElement>(HttpMethod.Post, $"questions/{questionId}/answers", new { content }, cancellationToken: cancellationToken);
    }

    private async Task<T> SendAsync<T>(
        HttpMethod method,
        string path,
        object? body = null,
        bool requiresAuth = true,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, BuildUri(path));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (requiresAuth)
        {
            if (string.IsNullOrWhiteSpace(_session.AccessToken))
            {
                throw new InvalidOperationException("Please sign in before calling protected APIs.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: _jsonOptions);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetErrorMessage(payload, response.StatusCode));
        }

        if (string.IsNullOrWhiteSpace(payload))
        {
            return default!;
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var data = root.TryGetProperty("data", out var dataElement) ? dataElement : root;

        var result = data.Deserialize<T>(_jsonOptions);
        return result ?? throw new InvalidOperationException("API response could not be parsed.");
    }

    private Uri BuildUri(string path)
    {
        var baseUrl = _session.ApiBaseUrl.TrimEnd('/') + "/";
        return new Uri(new Uri(baseUrl), path.TrimStart('/'));
    }

    private string GetErrorMessage(string payload, System.Net.HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return $"API request failed with status {(int)statusCode}.";
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("message", out var message))
            {
                return message.ValueKind == JsonValueKind.Array
                    ? string.Join(Environment.NewLine, message.EnumerateArray().Select(item => item.GetString()))
                    : message.GetString() ?? $"API request failed with status {(int)statusCode}.";
            }
        }
        catch (JsonException)
        {
            return payload;
        }

        return payload;
    }
}
