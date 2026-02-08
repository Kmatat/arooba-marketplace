using Arooba.Application.Common.Exceptions;
using Arooba.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Arooba.Application.Features.Auth.Commands;

/// <summary>
/// Command to verify an OTP code and authenticate the user.
/// Returns JWT token on successful verification.
/// </summary>
public record VerifyOtpCommand : IRequest<LoginResultDto>
{
    /// <summary>Gets the user's Egyptian mobile number.</summary>
    public string MobileNumber { get; init; } = default!;

    /// <summary>Gets the OTP code to verify.</summary>
    public string OtpCode { get; init; } = default!;
}

/// <summary>
/// Handles OTP verification and issues a JWT token on success.
/// </summary>
public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, LoginResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IOtpService _otpService;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeService _dateTime;

    public VerifyOtpCommandHandler(
        IApplicationDbContext context,
        IOtpService otpService,
        IIdentityService identityService,
        IDateTimeService dateTime)
    {
        _context = context;
        _otpService = otpService;
        _identityService = identityService;
        _dateTime = dateTime;
    }

    public async Task<LoginResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        // Verify OTP
        var verifyResult = await _otpService.VerifyOtpAsync(request.MobileNumber, request.OtpCode);

        if (!verifyResult.IsValid)
        {
            throw new BadRequestException(verifyResult.ErrorMessage ?? "Invalid OTP.");
        }

        // Retrieve user for token generation
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.MobileNumber, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User", request.MobileNumber);
        }

        if (!user.IsActive)
        {
            throw new ForbiddenAccessException("Your account has been deactivated. Please contact support.");
        }

        // Generate JWT token via identity service
        var authResult = await _identityService.CreateUserAsync(user.FullName, user.Id.ToString());

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
/// Validates the <see cref="VerifyOtpCommand"/>.
/// </summary>
public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(c => c.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+20(10|11|12|15)\d{8}$")
            .WithMessage("Mobile number must be a valid Egyptian mobile number (e.g., +201XXXXXXXXX).");

        RuleFor(c => c.OtpCode)
            .NotEmpty().WithMessage("OTP code is required.")
            .Length(6).WithMessage("OTP code must be 6 digits.")
            .Matches(@"^\d+$").WithMessage("OTP code must contain only digits.");
    }
}
