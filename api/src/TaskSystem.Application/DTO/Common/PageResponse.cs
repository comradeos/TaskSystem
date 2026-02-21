namespace TaskSystem.Application.DTO.Common;

public sealed class PageResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int Page { get; init; }

    public required int Size { get; init; }

    public required long Total { get; init; }
}