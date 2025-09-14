using FluentValidation;
using ProductApi.Api.DTOs.Requests;

namespace ProductApi.Api.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Product description cannot exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than zero")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Product price cannot exceed 999,999.99");

        RuleFor(x => x.StockAvailable)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock available cannot be negative")
            .LessThanOrEqualTo(int.MaxValue)
            .WithMessage("Stock available value is too large");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("Product category is required")
            .MaximumLength(100)
            .WithMessage("Product category cannot exceed 100 characters");
    }
}

