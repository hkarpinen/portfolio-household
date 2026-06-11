using FluentValidation;
using Household.Application.Dtos;

namespace Client.Validators;

public sealed class CreateChoreRequestValidator : AbstractValidator<CreateChoreRequest>
{
    public CreateChoreRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class AssignChoreRequestValidator : AbstractValidator<AssignChoreRequest>
{
    public AssignChoreRequestValidator()
    {
        RuleFor(x => x.AssignToUserId).NotEmpty();
    }
}
