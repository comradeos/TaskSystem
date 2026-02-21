namespace TaskSystem.Api.DTOs;

public class CreateTaskRequestDto
{
    public int ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Status { get; set; }

    public int Priority { get; set; }

    public int? AssignedUserId { get; set; }
}