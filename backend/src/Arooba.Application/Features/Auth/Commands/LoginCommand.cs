using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Auth.Commands;

/// <summary>
/// Command to authenticate a user using their Egyptian mobile number and OTP.
/// Returns authentication details including a JWT token concept.
/// </summary>
public record LoginCommand : IRequest<LoginResultDto>
{
    /// <summary>Gets the user's Egyptian mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the one-time password sent via SMS.</summary>
    public string Otp { get; init; } = default!;
}

/// <summary>
/// DTO containing authentication result details.
/// </summary>
public record LoginResultDto
{
    /// <summary>Gets the user identifier.</summary>
    public Guid UserId { get; init; }

    /// <summary>Gets the JWT access token.</summary>
    public string Token { get; init; } = default!;

    /// <summary>Gets the user's full name.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Gets the user's role.</summary>
    public string Role { get; init; } = default!;

    /// <summary>Gets the token expiration date.</summary>
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Handles user authentication via mobile number and OTP.
/// Validates the user exists and delegates to the identity service for OTP verification.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeService _dateTime;

    /// <summary>
    /// Initializes a new instance of <see cref="LoginCommandHandler"/>.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="identityService">The identity service for authentication.</param>
    /// <param name="dateTime">The date/time service.</param>
    public LoginCommandHandler(
        IApplicationDbContext context,
        IIdentityService identityService,
        IDateTimeService dateTime)
    {
        _context = context;
        _identityService = identityService;
        _dateTime = dateTime;
    }

    /// <summary>
    /// Authenticates the user by:
    /// 1. Verifying the mobile number exists in the system
    /// 2. Validating the OTP through the identity service
    /// 3. Returning authentication details with JWT token
    /// </summary>
    /// <param name="request">The login command with mobile number and OTP.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Authentication result with token and user details.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the given mobile number.</exception>
    /// <exception cref="BadRequestException">Thrown when OTP verification fails.</exception>
    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find the user by mobile number
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", request.MobileNumber);
        }

        if (!user.IsActive)
        {
            throw new ForbiddenAccessException("Your account has been deactivated. Please contact support.");
        }

        // Authenticate via identity service (OTP validation)
        var authResult = await _identityService.CreateUserAsync(user.FullName, request.Otp);

        if (!authResult.IsSuccess)
        {
            throw new BadRequestException("Invalid or expired OTP. Please request a new one.");
        }

        var now = _dateTime.UtcNow;

        return new LoginResultDto
        {
            UserId = user.Id,
            Token = authResult.Value ?? string.Empty,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            ExpiresAt = now.AddHours(24)
        };
    }
}

/// <summary>
/// Validates the <see cref="LoginCommand"/>.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes validation rules for the login command.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(l => l.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+20(10|11|12|15)\d{8}$")
            .WithMessage("Mobile number must be a valid Egyptian mobile number (e.g., +201XXXXXXXXX).");

        RuleFor(l => l.Otp)
            .NotEmpty().WithMessage("OTP is required.")
            .Length(4, 6).WithMessage("OTP must be between 4 and 6 digits.")
            .Matches(@"^\d+$").WithMessage("OTP must contain only digits.");
    }
}
