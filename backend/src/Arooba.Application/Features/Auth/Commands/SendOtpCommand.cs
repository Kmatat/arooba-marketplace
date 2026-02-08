using Arooba.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Arooba.Application.Features.Auth.Commands;

/// <summary>
/// Command to send a one-time password to a user's mobile number via ADVANSYS SMS gateway.
/// </summary>
public record SendOtpCommand : IRequest<SendOtpResultDto>
{
    /// <summary>Gets the user's Egyptian mobile number.</summary>
    public string MobileNumber { get; init; } = default!;
}

/// <summary>
/// DTO containing the result of an OTP send operation.
/// </summary>
public record SendOtpResultDto
{
    /// <summary>Whether the OTP was sent successfully.</summary>
    public bool Success { get; init; }

    /// <summary>A user-facing message about the result.</summary>
    public string Message { get; init; } = default!;

    /// <summary>Transaction ID from the SMS provider (for tracking/support).</summary>
    public string? TransactionId { get; init; }
}

/// <summary>
/// Handles sending an OTP to the user's mobile number.
/// </summary>
public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, SendOtpResultDto>
{
    private readonly IOtpService _otpService;

    public SendOtpCommandHandler(IOtpService otpService)
    {
        _otpService = otpService;
    }

    public async Task<SendOtpResultDto> Handle(SendOtpCommand request, CancellationToken cancellationToken)
    {
        var result = await _otpService.SendOtpAsync(request.MobileNumber);

        return new SendOtpResultDto
        {
            Success = result.Success,
            Message = result.Success
                ? "OTP sent successfully. Please check your SMS."
                : result.ErrorMessage ?? "Failed to send OTP.",
            TransactionId = result.TransactionId
        };
    }
}

/// <summary>
/// Validates the <see cref="SendOtpCommand"/>.
/// </summary>
public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(c => c.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Matches(@"^\+20(10|11|12|15)\d{8}$")
            .WithMessage("Mobile number must be a valid Egyptian mobile number (e.g., +201XXXXXXXXX).");
    }
}
