namespace TaskSystem.Api.DTOs;

public class UpdateTaskRequestDto
{
    public int? Status { get; set; }

    public int? Priority { get; set; }

    public int? AssignedUserId { get; set; }
}