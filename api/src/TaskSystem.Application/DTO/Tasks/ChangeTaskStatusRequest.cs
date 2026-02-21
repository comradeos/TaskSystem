using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.DTO.Tasks;

public sealed record ChangeTaskStatusRequest(TaskStatus Status);