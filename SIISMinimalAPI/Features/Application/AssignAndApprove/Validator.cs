using System;
using FluentValidation;
using SIISMinimalAPI.Features.Shared.Enums;

namespace SIISMinimalAPI.Features.Application.AssignAndApprove;

public class Validator: AbstractValidator<RequestDto>
{
    public Validator()
    {
        RuleFor(x => x.Office)
        .NotNull()
        .WithMessage("Office is required")
        .IsInEnum()
        .WithMessage("Invalid office selected.");
    }
}
