namespace TaskSystem.Api.DTOs;

public class CreateCommentRequestDto
{
    public int TaskId { get; set; }

    public string Content { get; set; } = string.Empty;
}