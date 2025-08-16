using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UserProfile.API.Shared.Global
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails details = CreateProblemDetails(httpContext, exception);
            httpContext.Response.StatusCode = details.Status ?? 500;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
            return true;
        }

        private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception ex)
        {
            var details = ex switch
            {
                ArgumentException args => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Bad Request",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = args.Message,
                    Instance = httpContext.Request.Path
                },

                _ => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Bad Request",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = ex.Message,
                    Instance = httpContext.Request.Path
                },
            };

            details.Extensions["traceId"] = httpContext.TraceIdentifier;

            return details;
        }
    }
}