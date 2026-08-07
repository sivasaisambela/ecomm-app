using FluentValidation;
using OrderService.Application.DTOs;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Validators
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required.")
                .Must(BeAValidOrderStatus)
                .WithMessage(x => $"'{x.Status}' is not a valid order status. Valid values: {string.Join(", ", Enum.GetNames(typeof(OrderStatus)))}");
        }

        private bool BeAValidOrderStatus(string status)
        {
            return Enum.TryParse<OrderStatus>(status, ignoreCase: true, out _);
        }
    }
}
