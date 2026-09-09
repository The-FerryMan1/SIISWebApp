using System;
using FluentValidation;

namespace SIISMinimalAPI.Features.RegistrationToken;

public class RegistrationTokenValidator : AbstractValidator<GenerateRegistrationTokenDto>
{
    public RegistrationTokenValidator()
    {
        RuleFor(x => x.ExpDate)
            .NotEmpty().WithMessage("Expiration date is required")
            .Must(DateRangeAllowed)
            .WithMessage("Expiration date must be in the future");

    }

    private static bool DateRangeAllowed(DateTime dob)
    {
        return dob.Date > DateTime.Today;
    }


}
