namespace TaskSystem.Domain.Common;

public class ApiResponse
{
    public bool Result { get; init; }

    public object? Data { get; init; }

    public static ApiResponse Success(object? data)
    {
        return new ApiResponse
        {
            Result = true,
            Data = data
        };
    }

    public static ApiResponse Failure(object problemDetails)
    {
        return new ApiResponse
        {
            Result = false,
            Data = problemDetails
        };
    }
}