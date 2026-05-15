using Client.Controllers;
using FluentValidation;

namespace Client.Validators;

public sealed class CreateCalendarEventRequestValidator : AbstractValidator<CreateCalendarEventRequest>
{
    public CreateCalendarEventRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt).When(x => x.EndsAt.HasValue)
            .WithMessage("EndsAt must be after StartsAt.");
    }
}

public sealed class UpdateCalendarEventRequestValidator : AbstractValidator<UpdateCalendarEventRequest>
{
    public UpdateCalendarEventRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.StartsAt).NotEmpty();
        RuleFor(x => x.EndsAt)
            .GreaterThan(x => x.StartsAt).When(x => x.EndsAt.HasValue)
            .WithMessage("EndsAt must be after StartsAt.");
    }
}
