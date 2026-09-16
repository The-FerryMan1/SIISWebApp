namespace SIISMinimalAPI.Features.Otp.Validators;

using FluentValidation;
using System.Net.Mail;

public sealed class SendOtpValidator : AbstractValidator<SendOtpDto>
{
    public SendOtpValidator()
    {
        RuleFor(t => t.Email)
            .NotEmpty()
            .Must(email => MailAddress.TryCreate(email, out _))
            .WithMessage("A valid email address is required.");
        RuleFor(t => t.RegistrationToken).NotEmpty();
    }
}