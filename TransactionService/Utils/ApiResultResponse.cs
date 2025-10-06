namespace TransactionService.Utils;

public class ApiResultResponse<T>
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public T? Data { get; private set; }

    public static ApiResultResponse<T> Success(T data)
    {
        return new ApiResultResponse<T> { IsSuccess = true, Data = data };
    }

    public static ApiResultResponse<T> Error(string message)
    {
        return new ApiResultResponse<T> { IsSuccess = false, Message = message };
    }

}
