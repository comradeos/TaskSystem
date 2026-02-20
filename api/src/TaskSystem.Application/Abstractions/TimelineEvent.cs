namespace TaskSystem.Application.Abstractions;

public sealed record TimelineEvent(
    string EntityType,
    string EntityId,
    string Action,
    object Data,
    DateTimeOffset OccurredAtUtc
);