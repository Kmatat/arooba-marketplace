using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Arooba.API.Filters;

/// <summary>
/// An alternative exception filter attribute that can be applied at the controller or action level
/// as a backup to the global <see cref="Middleware.ExceptionHandlingMiddleware"/>.
/// Converts known application exceptions into appropriate <see cref="ProblemDetails"/> responses.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly Dictionary<Type, Action<ExceptionContext>> _exceptionHandlers;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiExceptionFilterAttribute"/> class
    /// and registers all known exception-to-response mappings.
    /// </summary>
    public ApiExceptionFilterAttribute()
    {
        _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(Application.Common.Exceptions.NotFoundException), HandleNotFoundException },
            { typeof(Application.Common.Exceptions.ForbiddenAccessException), HandleForbiddenAccessException },
            { typeof(Application.Common.Exceptions.BadRequestException), HandleBadRequestException },
        };
    }

    /// <summary>
    /// Called when an exception is thrown during action execution. Attempts to map
    /// the exception to an appropriate HTTP response.
    /// </summary>
    /// <param name="context">The exception context containing the thrown exception.</param>
    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();

        if (_exceptionHandlers.TryGetValue(type, out var handler))
        {
            handler.Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
        {
            HandleInvalidModelStateException(context);
            return;
        }

        HandleUnknownException(context);
    }

    private static void HandleValidationException(ExceptionContext context)
    {
        var exception = (ValidationException)context.Exception;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var details = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleNotFoundException(ExceptionContext context)
    {
        var exception = context.Exception as Application.Common.Exceptions.NotFoundException;

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Status = StatusCodes.Status404NotFound,
            Detail = exception?.Message
        };

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleForbiddenAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Forbidden",
            Status = StatusCodes.Status403Forbidden,
            Detail = "You do not have permission to access this resource."
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status403Forbidden };
        context.ExceptionHandled = true;
    }

    private static void HandleBadRequestException(ExceptionContext context)
    {
        var exception = context.Exception as Application.Common.Exceptions.BadRequestException;

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception?.Message
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleInvalidModelStateException(ExceptionContext context)
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleUnknownException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred. Please try again later."
        };

        context.Result = new ObjectResult(details) { StatusCode = StatusCodes.Status500InternalServerError };
        context.ExceptionHandled = true;
    }
}
