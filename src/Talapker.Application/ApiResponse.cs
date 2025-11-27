namespace Talapker.Application;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }
    public int ErrorCode { get; set; }
    public string LocalizationKey{ get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public ApiResponse(T data)
    {
        IsSuccess = true;
        Data = data;
    }

    public ApiResponse(string error = "", string localizationKey = "null", int errorCode = 400)
    {
        IsSuccess = false;
        Error = error;
        LocalizationKey = localizationKey;
        ErrorCode = errorCode;
    }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>(data);
    }

    // Static factory methods for failure
    public static ApiResponse<T> Fail(string error = "", string localizationKey = "null", int errorCode = 400)
    {
        return new ApiResponse<T>(error, localizationKey, errorCode);
    }
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string Error{ get; set; }
    public string LocalizationKey{ get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ErrorCode { get; set; }

    public static ApiResponse Success()
    {
        return new ApiResponse { IsSuccess = true };
    }

    public static ApiResponse Fail(string error = "", string localizationKey = "null", int errorCode = 400)
    {
        return new ApiResponse
        {
            IsSuccess = false,
            Error = error,
            LocalizationKey = localizationKey,
            ErrorCode = errorCode
        };
    }
}