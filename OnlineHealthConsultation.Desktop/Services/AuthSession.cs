using OnlineHealthConsultation.Desktop.Models;

namespace OnlineHealthConsultation.Desktop.Services;

public sealed class AuthSession : IAuthSession
{
    public string ApiBaseUrl { get; set; } = "http://localhost:4000/api";
    public string? AccessToken { get; private set; }
    public UserDto? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public void SignIn(string accessToken, UserDto user)
    {
        AccessToken = accessToken;
        User = user;
    }

    public void SignOut()
    {
        AccessToken = null;
        User = null;
    }
}
