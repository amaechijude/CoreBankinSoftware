using src.Features.CustomerOnboarding;

namespace src.Features
{
    public abstract class BaseComandHandlerAsync<TCommand, TResult>
    {
        public abstract Task<ResultResponse<TResult>> HandleAsync(TCommand command);
    }

    public class ResultResponse<T>
    {
        public int StatusCode { get; private set; }
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ResultResponse<T> Success(T data) => new() { StatusCode=200 ,IsSuccess = true, Data = data };
        public static ResultResponse<T> Error(string errorMessage, int statusCode=400) => new() { StatusCode = statusCode, IsSuccess = false, ErrorMessage = errorMessage };
    }

}
