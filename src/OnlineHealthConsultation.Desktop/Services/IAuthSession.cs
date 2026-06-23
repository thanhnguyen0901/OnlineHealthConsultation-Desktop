using OnlineHealthConsultation.Desktop.Models;

namespace OnlineHealthConsultation.Desktop.Services;

public interface IAuthSession
{
    string ApiBaseUrl { get; set; }
    string? AccessToken { get; }
    UserDto? User { get; }
    bool IsAuthenticated { get; }
    void SignIn(string accessToken, UserDto user);
    void SignOut();
}
