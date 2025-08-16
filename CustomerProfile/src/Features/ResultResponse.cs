namespace UserProfile.API.Features
{
    public class ResultResponse<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public object? ErrorMessage { get; private set; }

        public static ResultResponse<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static ResultResponse<T> Error(object error) => new() { IsSuccess = false, ErrorMessage = error };
    }
}
