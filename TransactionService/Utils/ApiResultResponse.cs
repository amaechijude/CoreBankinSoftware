namespace TransactionService.Utils;

public class ApiResultResponse<T>
{
    public bool IsSuccess { get; private set; }
    public string? Message { get; private set; }
    public T? Data { get; private set; }
    public List<string>? Errors { get; private set; }

    public static ApiResultResponse<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static ApiResultResponse<T> Error(string message) =>
        new() { IsSuccess = false, Message = message };

    public static ApiResultResponse<T> Error(string message, List<string> errors) =>
        new()
        {
            IsSuccess = false,
            Message = message,
            Errors = errors,
        };
}
