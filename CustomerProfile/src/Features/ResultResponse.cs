using src.Features.CustomerOnboarding;

namespace src.Features
{
    public class ResultResponse<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ResultResponse<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static ResultResponse<T> Error(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
