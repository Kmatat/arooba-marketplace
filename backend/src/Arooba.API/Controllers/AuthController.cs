using Arooba.Application.Features.Auth.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Arooba.API.Controllers;

/// <summary>
/// Handles authentication operations including user registration, OTP verification,
/// social login (Google, Apple, Facebook), and token refresh.
/// </summary>
public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Registers a new user account. An OTP is automatically sent to the provided mobile number.
    /// </summary>
    /// <param name="command">The registration details including name, mobile number, email, and role.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>The user ID and OTP delivery status.</returns>
    /// <response code="201">User registered successfully. OTP sent for mobile verification.</response>
    /// <response code="400">Validation failed (e.g., duplicate mobile number).</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = result.UserId }, result);
    }

    /// <summary>
    /// Sends a one-time password to the user's mobile number via ADVANSYS SMS gateway.
    /// Used for login and mobile number verification.
    /// </summary>
    /// <param name="command">The mobile number to send the OTP to.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>OTP delivery status.</returns>
    /// <response code="200">OTP sent successfully.</response>
    /// <response code="400">Invalid mobile number or rate-limited.</response>
    [HttpPost("send-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SendOtpResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendOtp(
        [FromBody] SendOtpCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Verifies an OTP code and authenticates the user, returning a JWT bearer token.
    /// </summary>
    /// <param name="command">The mobile number and OTP code to verify.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A JWT token with user claims and expiration details.</returns>
    /// <response code="200">OTP verified; JWT token returned.</response>
    /// <response code="400">Invalid or expired OTP.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyOtp(
        [FromBody] VerifyOtpCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Authenticates a user using their mobile number and OTP (legacy login endpoint).
    /// Prefer using send-otp + verify-otp endpoints for new integrations.
    /// </summary>
    /// <param name="command">The login credentials (mobile number and OTP).</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A JWT token with user claims and expiration details.</returns>
    /// <response code="200">Login successful; token returned.</response>
    /// <response code="400">Invalid credentials.</response>
    /// <response code="404">User not found.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
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
    /// Authenticates or registers a user via social login (Google, Apple, Facebook).
    /// If the user is new, a mobile number must be provided and will need OTP verification.
    /// </summary>
    /// <param name="command">The social provider, token, and optional mobile number.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>JWT token and mobile verification status.</returns>
    /// <response code="200">Social login successful; token returned.</response>
    /// <response code="400">Invalid social token or missing required fields.</response>
    [HttpPost("social-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SocialLoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SocialLogin(
        [FromBody] SocialLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
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
