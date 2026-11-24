namespace HotelSaaS.API.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Error = null
        };
    }

    public static ApiResponse<T> Fail(string message, string? code = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Error = new ApiError { Code = code ?? "ERROR", Message = message }
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Data = null,
            Message = message,
            Error = null
        };
    }

    public static new ApiResponse Fail(string message, string? code = null)
    {
        return new ApiResponse
        {
            Success = false,
            Data = null,
            Message = message,
            Error = new ApiError { Code = code ?? "ERROR", Message = message }
        };
    }
}

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    public static PagedResponse<T> Create(IEnumerable<T> items, int totalCount, int page, int pageSize, string? message = null)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        return new PagedResponse<T>
        {
            Success = true,
            Data = items,
            Message = message,
            Error = null,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}

public class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IDictionary<string, string[]>? ValidationErrors { get; init; }
}
