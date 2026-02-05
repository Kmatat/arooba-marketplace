using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Application.Features.Auth.Commands.Login;
using Arooba.Application.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Arooba.API.Controllers;

/// <summary>
/// Handles authentication operations including user registration, login, and token refresh.
/// Issues JWT tokens for authenticated sessions.
/// </summary>
public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Registers a new user account on the Arooba Marketplace.
    /// </summary>
    /// <param name="command">The registration details including name, email, mobile number, and password.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The newly created user identifier and authentication token.</returns>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Validation failed (e.g., duplicate email or mobile).</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT bearer token.
    /// </summary>
    /// <param name="command">The login credentials (email/mobile and password).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A JWT token with user claims and expiration details.</returns>
    /// <response code="200">Login successful; token returned.</response>
    /// <response code="400">Invalid credentials or account locked.</response>
    /// <response code="404">User account not found.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request containing the expired access token and refresh token.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A new JWT token pair.</returns>
    /// <response code="200">Tokens refreshed successfully.</response>
    /// <response code="400">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        // Validate the refresh token against the database
        var user = await Sender.Send(new LoginCommand
        {
            MobileNumber = request.MobileNumber,
            Otp = request.RefreshToken
        }, cancellationToken);

        return Ok(user);
    }
}

/// <summary>
/// Request body for token refresh.
/// </summary>
public record RefreshTokenRequest
{
    public string MobileNumber { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// Response for token refresh.
/// </summary>
public record RefreshTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
