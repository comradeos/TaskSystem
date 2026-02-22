using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace TaskSystem.Api.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;

    public AuthService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<LoginResult?> LoginAsync(string login, string password)
    {
        User? user = await _userRepository.GetByLoginAsync(login);

        if (user is null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return null;
        }

        string token = Guid.NewGuid().ToString();
        
        Session session = new(0, user.Id, token, DateTime.UtcNow);

        await _sessionRepository.CreateAsync(session);
        
        LoginResult result = new(token, user.Id, user.Name, user.IsAdmin);

        return result;
    }

    public async Task LogoutAsync(string token)
    {
        await _sessionRepository.DeleteAsync(token);
    }
}