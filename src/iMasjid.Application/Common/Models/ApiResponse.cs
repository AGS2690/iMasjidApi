namespace iMasjid.Application.Common.Models;

public record ErrorDetail(string Field, string Message);

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IReadOnlyList<ErrorDetail>? Errors { get; set; }
    public string? CorrelationId { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully.")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> FailureResult(string message, IReadOnlyList<ErrorDetail>? errors = null, string? correlationId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            CorrelationId = correlationId
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult(string message = "Operation completed successfully.")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public static new ApiResponse FailureResult(string message, IReadOnlyList<ErrorDetail>? errors = null, string? correlationId = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors,
            CorrelationId = correlationId
        };
    }
}
