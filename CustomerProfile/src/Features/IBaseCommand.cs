using src.Features.CustomerOnboarding;

namespace src.Features
{
    public interface IBaseCommandHandlerAsync<TRequest, ResultResponse>
    {
        Task<ResultResponse> HandleAsync(TRequest command);
    }

    // Base command handler class that can be extended for specific command handling
    public class BaseComandHandlerAsync<TCommand, TResult> : IBaseCommandHandlerAsync<TCommand, ResultResponse<TResult>>
    {
        public virtual async Task<ResultResponse<TResult>> HandleAsync(TCommand command)
        {
            // Default implementation can be overridden by derived classes
            return await Task.FromResult(ResultResponse<TResult>.Error("Not implemented", 500));
        }
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
