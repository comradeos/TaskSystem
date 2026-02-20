using TaskSystem.Domain.Enums;
using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.DTO.Tasks;

public sealed class TaskCreateRequest
{
    public int ProjectId { get; init; }
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public int? AssigneeId { get; init; }
}