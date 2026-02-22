namespace TaskSystem.Api.Common;

public static class RequestValidator
{
    public static bool NotEmpty(string? value)
        => !string.IsNullOrWhiteSpace(value);

    public static bool EnumValid<TEnum>(int value)
        where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), value);

    public static bool NotNull(object? value)
        => value is not null;
}