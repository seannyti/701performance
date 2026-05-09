using FluentValidation;
using PerformancePower.Api.Controllers;

namespace PerformancePower.Api.Validators;

public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 2)
            .WithMessage($"Year must be between 1900 and {DateTime.UtcNow.Year + 2}.");

        RuleFor(x => x.Make)
            .NotEmpty().WithMessage("Make is required.")
            .MaximumLength(100);

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required.")
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.");

        RuleFor(x => x.Condition)
            .NotEmpty().WithMessage("Condition is required.");

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Cost cannot be negative.");

        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sale price cannot be negative.");

        RuleFor(x => x.Msrp)
            .GreaterThanOrEqualTo(0).WithMessage("MSRP cannot be negative.");
    }
}
