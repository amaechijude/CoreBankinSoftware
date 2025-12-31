namespace CustomerProfile.DTO;

public class ApiResponse<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public object? ErrorMessage { get; private set; }

    public static ApiResponse<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static ApiResponse<T> Error(object error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
