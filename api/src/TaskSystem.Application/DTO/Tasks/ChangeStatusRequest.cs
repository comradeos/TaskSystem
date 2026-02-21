using TaskStatus = TaskSystem.Domain.Enums.TaskStatus;

namespace TaskSystem.Application.DTO.Tasks;

public sealed record ChangeStatusRequest(TaskStatus Status);