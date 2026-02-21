namespace TaskSystem.Domain.Entities;

public class User
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public string Login { get; private set; }

    public string PasswordHash { get; private set; }

    public bool IsAdmin { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public User(
        int id,
        string name,
        string login,
        string passwordHash,
        bool isAdmin,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        Login = login;
        PasswordHash = passwordHash;
        IsAdmin = isAdmin;
        CreatedAt = createdAt;
    }
}