using OnlineHealthConsultation.Desktop.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineHealthConsultation.Desktop.Services;

public sealed class ApiClient : IApiClient
{
    private readonly IAuthSession _session;
    private readonly bool _useMockApi;
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly List<AppointmentDto> _mockAppointments = new()
    {
        new AppointmentDto
        {
            Id = "apt-1",
            Status = "PENDING_CONFIRMATION",
            ScheduledAt = DateTimeOffset.Now.AddHours(2),
            Reason = "Tư vấn chế độ dinh dưỡng cho người cao tuổi bị tiểu đường.",
            Notes = "Cần tư vấn kỹ về khẩu phần ăn hàng ngày.",
            Patient = new PatientProfileDto
            {
                Id = "pat-1",
                Phone = "0901234567",
                Gender = "MALE",
                DateOfBirth = DateTimeOffset.Now.AddYears(-65),
                User = new UserDto { Id = "u-pat-1", Email = "patient.e2e@healthcare.local", FirstName = "Hải", LastName = "Nguyễn" }
            }
        },
        new AppointmentDto
        {
            Id = "apt-2",
            Status = "CONFIRMED",
            ScheduledAt = DateTimeOffset.Now.AddHours(4),
            Reason = "Đau ngực nhẹ khi vận động mạnh.",
            Notes = "Đã có lịch sử tăng huyết áp 3 năm.",
            Patient = new PatientProfileDto
            {
                Id = "pat-2",
                Phone = "0987654321",
                Gender = "FEMALE",
                DateOfBirth = DateTimeOffset.Now.AddYears(-50),
                User = new UserDto { Id = "u-pat-2", Email = "patient2@healthcare.local", FirstName = "Lan", LastName = "Trần" }
            }
        },
        new AppointmentDto
        {
            Id = "apt-3",
            Status = "COMPLETED",
            ScheduledAt = DateTimeOffset.Now.AddDays(-1),
            Reason = "Tư vấn kết quả xét nghiệm máu.",
            Notes = "Đã kê đơn thuốc hạ mỡ máu.",
            Patient = new PatientProfileDto
            {
                Id = "pat-3",
                Phone = "0912345678",
                Gender = "MALE",
                DateOfBirth = DateTimeOffset.Now.AddYears(-40),
                User = new UserDto { Id = "u-pat-3", Email = "patient3@healthcare.local", FirstName = "Minh", LastName = "Lê" }
            }
        }
    };

    private readonly List<QuestionDto> _mockQuestions = new()
    {
        new QuestionDto
        {
            Id = "q-1",
            Title = "Triệu chứng đau đầu kéo dài",
            Content = "Tôi bị đau nửa đầu bên phải khoảng 3 ngày nay, uống thuốc giảm đau paracetamol chỉ đỡ vài tiếng rồi bị lại. Tôi nên làm gì?",
            Status = "PENDING",
            CreatedAt = DateTimeOffset.Now.AddHours(-5),
            Patient = new PatientProfileDto
            {
                Id = "pat-1",
                User = new UserDto { FirstName = "Hải", LastName = "Nguyễn" }
            }
        },
        new QuestionDto
        {
            Id = "q-2",
            Title = "Cách dùng thuốc huyết áp",
            Content = "Bác sĩ cho hỏi nên uống thuốc hạ huyết áp lúc sáng sớm hay buổi tối trước khi đi ngủ là tốt nhất?",
            Status = "PENDING",
            CreatedAt = DateTimeOffset.Now.AddHours(-12),
            Patient = new PatientProfileDto
            {
                Id = "pat-2",
                User = new UserDto { FirstName = "Lan", LastName = "Trần" }
            }
        }
    };

    public ApiClient(IAuthSession session, IConfiguration configuration)
    {
        _session = session;
        _useMockApi = configuration.GetValue<bool>("UseMockApi");
    }

    public async Task<AuthResultDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(500, cancellationToken);
            return new AuthResultDto
            {
                AccessToken = "mock-jwt-token-for-testing",
                User = new UserDto
                {
                    Id = "mock-doctor-id",
                    Email = email,
                    FirstName = "An",
                    LastName = "Nguyễn",
                    Role = "DOCTOR"
                }
            };
        }

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

    public async Task<DoctorProfileDto> GetDoctorProfileAsync(CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(300, cancellationToken);
            return new DoctorProfileDto
            {
                Id = "mock-doctor-profile-id",
                Bio = "Bác sĩ chuyên khoa Nội tổng quát với kinh nghiệm khám bệnh mạn tính, tư vấn dùng thuốc an toàn và theo dõi sức khỏe gia đình.",
                YearsOfExperience = 12,
                AvgRating = 4.8m,
                RatingCount = 24,
                User = new UserDto
                {
                    Id = "mock-doctor-id",
                    Email = _session.User?.Email ?? "doctor.mock@healthcare.local",
                    FirstName = _session.User?.FirstName ?? "An",
                    LastName = _session.User?.LastName ?? "Nguyễn",
                    Role = "DOCTOR"
                },
                Specialties = new List<DoctorSpecialtyLinkDto>
                {
                    new() { Specialty = new SpecialtyDto { Id = "specialty-1", NameVi = "Nội tổng quát", NameEn = "General Medicine" } },
                    new() { Specialty = new SpecialtyDto { Id = "specialty-2", NameVi = "Dinh dưỡng", NameEn = "Nutrition" } }
                }
            };
        }

        return await SendAsync<DoctorProfileDto>(HttpMethod.Get, "doctors/me/profile", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetDoctorAppointmentsAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(200, cancellationToken);
            if (string.IsNullOrWhiteSpace(status) || status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                return _mockAppointments;
            }
            return _mockAppointments.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var path = string.IsNullOrWhiteSpace(status)
            ? "appointments/doctor/me"
            : $"appointments/doctor/me?status={Uri.EscapeDataString(status)}";

        return await SendAsync<List<AppointmentDto>>(HttpMethod.Get, path, cancellationToken: cancellationToken);
    }

    public async Task<AppointmentDto> ConfirmAppointmentAsync(string appointmentId, CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(200, cancellationToken);
            var apt = _mockAppointments.FirstOrDefault(a => a.Id == appointmentId);
            if (apt != null)
            {
                apt.Status = "CONFIRMED";
            }
            return apt ?? throw new InvalidOperationException("Appointment not found.");
        }

        return await SendAsync<AppointmentDto>(HttpMethod.Patch, $"appointments/{appointmentId}/confirm", cancellationToken: cancellationToken);
    }

    public async Task<AppointmentDto> CompleteAppointmentAsync(string appointmentId, CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(200, cancellationToken);
            var apt = _mockAppointments.FirstOrDefault(a => a.Id == appointmentId);
            if (apt != null)
            {
                apt.Status = "COMPLETED";
            }
            return apt ?? throw new InvalidOperationException("Appointment not found.");
        }

        return await SendAsync<AppointmentDto>(HttpMethod.Patch, $"appointments/{appointmentId}/complete", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<QuestionDto>> GetAssignedQuestionsAsync(CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(200, cancellationToken);
            return _mockQuestions.Where(q => q.Status == "PENDING").ToList();
        }

        return await SendAsync<List<QuestionDto>>(HttpMethod.Get, "questions/assigned", cancellationToken: cancellationToken);
    }

    public async Task AnswerQuestionAsync(string questionId, string content, CancellationToken cancellationToken = default)
    {
        if (_useMockApi)
        {
            await Task.Delay(300, cancellationToken);
            var q = _mockQuestions.FirstOrDefault(x => x.Id == questionId);
            if (q != null)
            {
                q.Status = "ANSWERED";
                q.Answers.Add(new AnswerDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = content,
                    CreatedAt = DateTimeOffset.Now
                });
            }
            return;
        }

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
