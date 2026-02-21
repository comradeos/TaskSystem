namespace TaskSystem.Application.DTO.Common;

public sealed class PageRequest
{
    public int Page { get; init; } = 1;

    public int Size { get; init; } = 20;
}