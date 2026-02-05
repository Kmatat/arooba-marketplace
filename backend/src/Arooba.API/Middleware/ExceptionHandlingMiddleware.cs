using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Arooba.API.Middleware;

/// <summary>
/// Global exception-handling middleware that intercepts unhandled exceptions thrown anywhere
/// in the request pipeline and converts them into consistent, RFC 7807-compliant JSON responses.
/// </summary>
/// <remarks>
/// <para>Exception mapping:</para>
/// <list type="bullet">
///   <item><see cref="ValidationException"/> from FluentValidation produces <c>400 Bad Request</c> with an errors dictionary.</item>
///   <item><see cref="Application.Common.Exceptions.NotFoundException"/> produces <c>404 Not Found</c>.</item>
///   <item><see cref="Application.Common.Exceptions.ForbiddenAccessException"/> produces <c>403 Forbidden</c>.</item>
///   <item><see cref="Application.Common.Exceptions.BadRequestException"/> produces <c>400 Bad Request</c>.</item>
///   <item>All other exceptions produce <c>500 Internal Server Error</c> (with details only in Development).</item>
/// </list>
/// </remarks>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Logger for recording exception details.</param>
    /// <param name="environment">The hosting environment, used to toggle detail exposure.</param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invokes the middleware, wrapping the downstream pipeline in exception handling.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, type, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation Failed",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())
            ),

            Application.Common.Exceptions.NotFoundException => (
                HttpStatusCode.NotFound,
                "Not Found",
                "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                (Dictionary<string, string[]>?)null
            ),

            Application.Common.Exceptions.ForbiddenAccessException => (
                HttpStatusCode.Forbidden,
                "Forbidden",
                "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                (Dictionary<string, string[]>?)null
            ),

            Application.Common.Exceptions.BadRequestException badRequestEx => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                new Dictionary<string, string[]>
                {
                    { "Error", new[] { badRequestEx.Message } }
                }
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                (Dictionary<string, string[]>?)null
            )
        };

        // In non-development environments, never leak internal details for 500 errors.
        if (statusCode == HttpStatusCode.InternalServerError && !_environment.IsDevelopment())
        {
            errors = null;
        }
        else if (statusCode == HttpStatusCode.InternalServerError && _environment.IsDevelopment())
        {
            errors = new Dictionary<string, string[]>
            {
                { "Exception", new[] { exception.Message } },
                { "StackTrace", new[] { exception.StackTrace ?? string.Empty } }
            };
        }

        var response = new ErrorResponse
        {
            Type = type,
            Title = title,
            Status = (int)statusCode,
            Errors = errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    /// <summary>
    /// Standard error response envelope conforming to RFC 7807 problem details.
    /// </summary>
    private sealed class ErrorResponse
    {
        /// <summary>A URI reference that identifies the problem type.</summary>
        public string Type { get; init; } = string.Empty;

        /// <summary>A short, human-readable summary of the problem.</summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>The HTTP status code.</summary>
        public int Status { get; init; }

        /// <summary>A dictionary of validation or error messages keyed by property name.</summary>
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
