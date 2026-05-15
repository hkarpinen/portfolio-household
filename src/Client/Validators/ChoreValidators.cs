using Client.Controllers;
using FluentValidation;

namespace Client.Validators;

public sealed class CreateChoreRequestValidator : AbstractValidator<CreateChoreRequest>
{
    public CreateChoreRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
