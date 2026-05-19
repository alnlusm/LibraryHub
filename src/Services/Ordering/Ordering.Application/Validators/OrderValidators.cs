using FluentValidation;
using Ordering.Application.Orders;

namespace Ordering.Application.Validators;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.StartDateUtc).NotEmpty();
        RuleFor(x => x.EndDateUtc).GreaterThan(x => x.StartDateUtc);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemRequestValidator());
    }
}

public sealed class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        RuleFor(x => x.BookId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(10);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
