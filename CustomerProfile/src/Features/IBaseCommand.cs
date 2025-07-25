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
        public virtual Task<ResultResponse<TResult>> HandleAsync(TCommand command)
        {
            // Default implementation can be overridden by derived classes
            return Task.FromResult(ResultResponse<TResult>.Error("Not implemented"));
        }
    }

    public class ResultResponse<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ResultResponse<T> Success(T data) => new() { IsSuccess = true, Data = data };
        public static ResultResponse<T> Error(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
