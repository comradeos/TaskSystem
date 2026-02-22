namespace TaskSystem.Api.Common;

public static class RequestValidator
{
    public static bool NotEmpty(string? value)
        => !string.IsNullOrWhiteSpace(value);

    public static bool NotNull(object? value)
        => value is not null;
}