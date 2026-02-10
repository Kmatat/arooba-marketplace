using System.Security.Claims;
using Arooba.Domain.Enums;
using DomainInterfaces = Arooba.Domain.Interfaces;
using AppInterfaces = Arooba.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Arooba.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="DomainInterfaces.ICurrentUserService"/> and
/// <see cref="AppInterfaces.ICurrentUserService"/> that reads the authenticated user's
/// identity from the current HTTP context via <see cref="IHttpContextAccessor"/>.
/// </summary>
public class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : DomainInterfaces.ICurrentUserService, AppInterfaces.ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user, or <c>null</c> if unauthenticated.
    /// </summary>
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Gets the role of the currently authenticated user as a <see cref="UserRole"/> enum,
    /// or <c>null</c> if unauthenticated or role cannot be parsed.
    /// Implements <see cref="DomainInterfaces.ICurrentUserService.UserRole"/>.
    /// </summary>
    UserRole? DomainInterfaces.ICurrentUserService.UserRole
    {
        get
        {
            var roleString = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return roleString is not null && Enum.TryParse<UserRole>(roleString, true, out var role)
                ? role
                : null;
        }
    }

    /// <summary>
    /// Gets the role of the currently authenticated user as a string,
    /// or <c>null</c> if unauthenticated.
    /// Implements <see cref="AppInterfaces.ICurrentUserService.UserRole"/>.
    /// </summary>
    string? AppInterfaces.ICurrentUserService.UserRole =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    /// <summary>
    /// Gets a value indicating whether a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
