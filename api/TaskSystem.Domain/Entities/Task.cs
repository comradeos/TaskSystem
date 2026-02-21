using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Domain.Entities;

public class Task
{
    public int Id { get; private set; }

    public int ProjectId { get; private set; }

    public int NumberInProject { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public TaskStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public int AuthorUserId { get; private set; }

    public string AuthorUserName { get; private set; }

    public int? AssignedUserId { get; private set; }

    public string? AssignedUserName { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Task(
        int id,
        int projectId,
        int numberInProject,
        string title,
        string? description,
        TaskStatus status,
        TaskPriority priority,
        int authorUserId,
        string authorUserName,
        int? assignedUserId,
        string? assignedUserName,
        DateTime createdAt,
        DateTime updatedAt)
    {
        Id = id;
        ProjectId = projectId;
        NumberInProject = numberInProject;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        AuthorUserId = authorUserId;
        AuthorUserName = authorUserName;
        AssignedUserId = assignedUserId;
        AssignedUserName = assignedUserName;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}