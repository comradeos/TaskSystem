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

    public async Task<string?> LoginAsync(string login, string password)
    {
        var user = await _userRepository.GetByLoginAsync(login);

        if (user is null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var token = Guid.NewGuid().ToString();

        await _sessionRepository.CreateAsync(new Session(
            0,
            user.Id,
            token,
            DateTime.UtcNow));

        return token;
    }

    public async Task LogoutAsync(string token)
    {
        await _sessionRepository.DeleteAsync(token);
    }
}