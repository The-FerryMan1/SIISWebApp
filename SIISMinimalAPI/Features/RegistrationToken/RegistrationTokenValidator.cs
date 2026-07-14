using System;
using FluentValidation;

namespace SIISMinimalAPI.Features.RegistrationToken;

public class RegistrationTokenValidator : AbstractValidator<GenerateRegistrationTokenDto>
{
    public RegistrationTokenValidator()
    {
        RuleFor(x => x.ExpDate)
        .NotEmpty()
        .Must(DateRangeAllowed)
        .WithMessage("Expiration date is required");

    }

    private static bool DateRangeAllowed(DateTime dob)
    {
        return dob.Date > DateTime.Today;
    }


}
