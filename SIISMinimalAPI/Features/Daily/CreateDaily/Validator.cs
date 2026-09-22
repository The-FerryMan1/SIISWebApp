using FluentValidation;

namespace SIISMinimalAPI.Features.Daily.CreateDaily;

public class Validator: AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Dailies)
            .NotEmpty()
            .WithMessage("At least one daily record is required.");

        RuleForEach(x => x.Dailies)
            .SetValidator(new DailyValidator());
    }
}

public class DailyValidator : AbstractValidator<Schema>
{
    public DailyValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty();

        RuleFor(x => x.Activities)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Hours)
            .InclusiveBetween(1, 24);

        RuleFor(x => x.InCharge)
            .MaximumLength(255);

        RuleFor(x => x.Remarks)
            .MaximumLength(1000);

        RuleFor(x => x.IncidentReport)
            .MaximumLength(2000);
    }
}