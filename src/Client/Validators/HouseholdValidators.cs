using Client.Controllers;
using FluentValidation;

namespace Client.Validators;

public sealed class CreateHouseholdRequestValidator : AbstractValidator<CreateHouseholdRequest>
{
    public CreateHouseholdRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CurrencyCode).Length(3).When(x => !string.IsNullOrEmpty(x.CurrencyCode));
    }
}

public sealed class UpdateHouseholdRequestValidator : AbstractValidator<UpdateHouseholdRequest>
{
    public UpdateHouseholdRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.CurrencyCode).Length(3).When(x => !string.IsNullOrEmpty(x.CurrencyCode));
    }
}
