using System.Security.Claims;
using Arooba.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICurrentUserService"/> that reads the authenticated user's
/// identity from the current HTTP context via <see cref="IHttpContextAccessor"/>.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <inheritdoc />
    public string? UserRole =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
