using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Domain.Entities;

public sealed class TaskItem
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public int? AssigneeId { get; set; }
    public DateTime CreatedAt { get; init; }
    
    public void Assign(int assigneeId)
    {
        if (assigneeId <= 0)
        {
            throw new ArgumentException("AssigneeId must be positive.");
        }

        if (AssigneeId == assigneeId)
        {
            return;
        }

        AssigneeId = assigneeId;
    }
}