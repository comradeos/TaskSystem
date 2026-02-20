using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus; // бо буде конфлікт з System.Threading.Tasks.TaskStatus

namespace TaskSystem.Domain.Entities;

public class TaskItem
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public int? AssigneeId { get; init; }
    public DateTime CreatedAt { get; init; }
}