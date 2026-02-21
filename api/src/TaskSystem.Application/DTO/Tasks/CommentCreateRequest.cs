namespace TaskSystem.Application.DTO.Tasks;

public sealed class CommentCreateRequest
{
    public string Text { get; init; } = null!;
}