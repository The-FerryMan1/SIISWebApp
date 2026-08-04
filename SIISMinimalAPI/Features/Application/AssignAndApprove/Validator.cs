using System;
using FluentValidation;

namespace SIISMinimalAPI.Features.Application.AssignAndApprove;

public class Validator: AbstractValidator<RequestDto>
{
    public Validator()
    {
        RuleFor(x => x.Office)
        .NotEmpty()
        .WithMessage("Office is required");
    }
}
