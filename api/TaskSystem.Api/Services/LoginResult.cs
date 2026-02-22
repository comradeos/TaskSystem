namespace TaskSystem.Api.Services;

public class LoginResult
{
    public string SessionToken { get; }
    public int Id { get; }
    public string Name { get; }
    public bool IsAdmin { get; }

    public LoginResult(
        string sessionToken,
        int id,
        string name,
        bool isAdmin)
    {
        SessionToken = sessionToken;
        Id = id;
        Name = name;
        IsAdmin = isAdmin;
    }
}