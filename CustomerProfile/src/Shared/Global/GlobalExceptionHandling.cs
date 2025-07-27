using Microsoft.AspNetCore.Diagnostics;

namespace src.Shared.Global
{
    public class GlobalExceptionHandling : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
