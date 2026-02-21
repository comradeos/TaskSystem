namespace TaskSystem.Domain.Entities;

public class TaskComment
{
    public int Id { get; }
    public int TaskId { get; }
    public int AuthorId { get; }
    public string Text { get; }
    public DateTime CreatedAtUtc { get; }

    public TaskComment(
        int id,
        int taskId,
        int authorId,
        string text,
        DateTime createdAtUtc
    )
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Comment text cannot be empty.",
                nameof(text)
            );
        }

        Id = id;
        TaskId = taskId;
        AuthorId = authorId;
        Text = text;
        CreatedAtUtc = createdAtUtc;
    }
}