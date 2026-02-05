using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Abstract base controller for all Arooba API controllers.
/// Provides common configuration including route template, API behavior,
/// and access to the MediatR <see cref="ISender"/> for dispatching commands and queries.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _sender;

    /// <summary>
    /// Gets the MediatR sender resolved from the request's service scope.
    /// Used by derived controllers to dispatch commands and queries.
    /// </summary>
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
