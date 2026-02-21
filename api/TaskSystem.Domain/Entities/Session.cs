namespace TaskSystem.Domain.Entities;

public class Session
{
    public int Id { get; private set; }

    public int UserId { get; private set; }

    public string SessionToken { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Session(
        int id,
        int userId,
        string sessionToken,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        SessionToken = sessionToken;
        CreatedAt = createdAt;
    }
}