using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Auth.Commands;

/// <summary>
/// Command to authenticate or register a user via social login (Google, Apple, Facebook).
/// The user must still provide and verify their mobile number via OTP separately.
/// </summary>
public record SocialLoginCommand : IRequest<SocialLoginResultDto>
{
    /// <summary>The social authentication provider.</summary>
    public SocialProvider Provider { get; init; }

    /// <summary>The ID token (Google/Apple) or access token (Facebook) from the social provider.</summary>
    public string Token { get; init; } = default!;

    /// <summary>The user's Egyptian mobile number (required for new registrations).</summary>
    public string? MobileNumber { get; init; }

    /// <summary>The user's full name (used if not available from the provider).</summary>
    public string? FullName { get; init; }
}

/// <summary>
/// Result of a social login operation.
/// </summary>
public record SocialLoginResultDto
{
    /// <summary>The user's ID.</summary>
    public int UserId { get; init; }

    /// <summary>The JWT access token.</summary>
    public string Token { get; init; } = default!;

    /// <summary>The user's full name.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>The user's role.</summary>
    public string Role { get; init; } = default!;

    /// <summary>Token expiration date.</summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>Whether this is a new user who needs to provide and verify their mobile number.</summary>
    public bool RequiresMobileVerification { get; init; }

    /// <summary>Whether the user's mobile number is verified.</summary>
    public bool IsMobileVerified { get; init; }
}

/// <summary>
/// Handles social login by validating the social token, finding or creating the user,
/// and issuing a JWT token.
/// </summary>
public class SocialLoginCommandHandler : IRequestHandler<SocialLoginCommand, SocialLoginResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ISocialAuthService _socialAuthService;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeService _dateTime;

    public SocialLoginCommandHandler(
        IApplicationDbContext context,
        ISocialAuthService socialAuthService,
        IIdentityService identityService,
        IDateTimeService dateTime)
    {
        _context = context;
        _socialAuthService = socialAuthService;
        _identityService = identityService;
        _dateTime = dateTime;
    }

    public async Task<SocialLoginResultDto> Handle(SocialLoginCommand request, CancellationToken cancellationToken)
    {
        // Validate the social token
        var socialResult = await _socialAuthService.ValidateTokenAsync(request.Provider, request.Token);

        if (!socialResult.Success)
        {
            throw new BadRequestException(socialResult.ErrorMessage ?? "Social login validation failed.");
        }

        // Try to find an existing user by social provider ID
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.SocialProvider == request.Provider && u.SocialProviderId == socialResult.ProviderId,
                cancellationToken);

        // If not found by provider, try by email
        if (user is null && !string.IsNullOrEmpty(socialResult.Email))
        {
            user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == socialResult.Email, cancellationToken);

            // Link the social provider to the existing account
            if (user is not null)
            {
                user.SocialProvider = request.Provider;
                user.SocialProviderId = socialResult.ProviderId;
                if (!string.IsNullOrEmpty(socialResult.AvatarUrl))
                {
                    user.AvatarUrl = socialResult.AvatarUrl;
                }
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var isNewUser = false;
        var now = _dateTime.UtcNow;

        if (user is null)
        {
            // New user registration via social login
            if (string.IsNullOrEmpty(request.MobileNumber))
            {
                throw new BadRequestException(
                    "Mobile number is required for new user registration. " +
                    "Please provide your Egyptian mobile number.");
            }

            // Check if mobile number is already taken
            var mobileExists = await _context.Users
                .AnyAsync(u => u.PhoneNumber == request.MobileNumber, cancellationToken);

            if (mobileExists)
            {
                throw new BadRequestException(
                    "This mobile number is already registered. " +
                    "Please login with your existing account or use a different number.");
            }

            var userId = new int();
            var fullName = socialResult.FullName ?? request.FullName ?? "User";

            user = new User
            {
                Id = userId,
                FullName = fullName,
                Email = socialResult.Email ?? string.Empty,
                PhoneNumber = request.MobileNumber,
                Role = UserRole.Customer,
                IsActive = true,
                SocialProvider = request.Provider,
                SocialProviderId = socialResult.ProviderId,
                AvatarUrl = socialResult.AvatarUrl,
                CreatedAt = now
            };

            _context.Users.Add(user);

            // Create customer record
            var customer = new Customer
            {
                Id = new int(),
                UserId = userId,
                FullName = fullName,
                MobileNumber = request.MobileNumber,
                Email = socialResult.Email,
                PreferredLanguage = "ar",
                CreatedAt = now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            isNewUser = true;
        }

        if (!user.IsActive)
        {
            throw new ForbiddenAccessException("Your account has been deactivated. Please contact support.");
        }

        // Update last login
        user.LastLoginAt = now;
        await _context.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var authResult = await _identityService.CreateUserAsync(user.FullName, user.Id.ToString());

        return new SocialLoginResultDto
        {
            UserId = user.Id,
            Token = authResult.Value ?? string.Empty,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            ExpiresAt = now.AddHours(24),
            RequiresMobileVerification = isNewUser || !user.IsMobileVerified,
            IsMobileVerified = user.IsMobileVerified
        };
    }
}

/// <summary>
/// Validates the <see cref="SocialLoginCommand"/>.
/// </summary>
public class SocialLoginCommandValidator : AbstractValidator<SocialLoginCommand>
{
    public SocialLoginCommandValidator()
    {
        RuleFor(c => c.Provider)
            .IsInEnum().WithMessage("A valid social provider is required.")
            .Must(p => p != SocialProvider.None).WithMessage("Social provider cannot be 'None'.");

        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Social authentication token is required.");

        RuleFor(c => c.MobileNumber)
            .Matches(@"^\+20(10|11|12|15)\d{8}$")
            .When(c => !string.IsNullOrEmpty(c.MobileNumber))
            .WithMessage("Mobile number must be a valid Egyptian mobile number (e.g., +201XXXXXXXXX).");
    }
}
