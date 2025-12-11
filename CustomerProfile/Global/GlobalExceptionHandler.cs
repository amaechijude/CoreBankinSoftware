using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using CustomerAPI.Messaging;

namespace CustomerAPI.Global
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            ProblemDetails details = CreateProblemDetails(httpContext, exception);
            httpContext.Response.StatusCode = details.Status ?? 500;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(details, cancellationToken);
            _logger.LogError("Exception occured: {Message}", exception.Message);
            return true;
        }

        private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception ex)
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
                ValidationException validation => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation Error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)),
                    Instance = httpContext.Request.Path
                },
                NotSupportedException args => new ProblemDetails
                {
                    Type = "Image type not supported",
                    Title = "Bad Request",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = args.Message,
                    Instance = httpContext.Request.Path
                },
                ServiceException service => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    Title = "Service Error",
                    Status = (int)HttpStatusCode.BadRequest,
                    Detail = service.Message,
                    Instance = httpContext.Request.Path
                },
                CustomTwilloException twilio => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Twilio Service Error",
                    Status = (int)HttpStatusCode.ServiceUnavailable,
                    Detail = twilio.Message,
                    Instance = httpContext.Request.Path
                },
                UnauthorizedAccessException => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Title = "Unauthorized",
                    Status = (int)HttpStatusCode.Unauthorized,
                    Detail = "You are not authorized to access this resource",
                    Instance = httpContext.Request.Path
                },
                _ => new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Title = "Internal Server Error",
                    Status = (int)HttpStatusCode.InternalServerError,
                    Detail = "An unexpected error occurred",
                    Instance = httpContext.Request.Path
                },
            };

            details.Extensions["traceId"] = httpContext.TraceIdentifier;
            if (ex is ValidationException validationEx)
            {
                details.Extensions["errors"] = validationEx.Errors;
            }

            return details;
        }
    }

    internal sealed class ServiceException(string message) : Exception(message);
}