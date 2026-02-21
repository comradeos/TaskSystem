namespace TaskSystem.Domain.Entities;

public class Comment
{
    public int Id { get; private set; }

    public int TaskId { get; private set; }

    public int UserId { get; private set; }

    public string UserName { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Comment(
        int id,
        int taskId,
        int userId,
        string userName,
        string content,
        DateTime createdAt)
    {
        Id = id;
        TaskId = taskId;
        UserId = userId;
        UserName = userName;
        Content = content;
        CreatedAt = createdAt;
    }
}