using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

public sealed class TaskItem
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public int Number { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; init; }
    public int? AssigneeId { get; private set; }
    public DateTime CreatedAt { get; init; }

    public bool Assign(int assigneeId)
    {
        if (assigneeId <= 0)
            throw new ArgumentException("AssigneeId must be positive.");

        if (AssigneeId == assigneeId)
            return false; // ничего не изменилось

        AssigneeId = assigneeId;

        if (Status == TaskStatus.Open)
            Status = TaskStatus.InProgress;

        return true;
    }

    public bool ChangeStatus(TaskStatus newStatus)
    {
        if (Status == newStatus)
            return false;

        // простая бизнес-валидация
        if (Status == TaskStatus.Done && newStatus != TaskStatus.Done)
            throw new InvalidOperationException("Completed task cannot change status.");

        Status = newStatus;
        return true;
    }
}