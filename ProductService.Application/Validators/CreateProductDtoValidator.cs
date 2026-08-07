using FluentValidation;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

            RuleFor(p => p.Sku)
                .NotEmpty().WithMessage("Product SKU is required.")
                .MaximumLength(50).WithMessage("Product SKU cannot exceed 50 characters.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("Product price must be greater than zero.");

            RuleFor(p => p.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Initial stock quantity cannot be negative.");

            RuleFor(p => p.Category)
                .NotEmpty().WithMessage("Product category is required.")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");
        }
    }
}
