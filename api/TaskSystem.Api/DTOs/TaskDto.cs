namespace TaskSystem.Api.DTOs;

public class TaskDto
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int NumberInProject { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Status { get; set; }

    public int Priority { get; set; }

    public int AuthorUserId { get; set; }

    public string AuthorUserName { get; set; } = string.Empty;

    public int? AssignedUserId { get; set; }

    public string? AssignedUserName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}