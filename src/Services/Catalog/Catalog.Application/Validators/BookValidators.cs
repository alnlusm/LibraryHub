using Catalog.Application.Books;
using FluentValidation;

namespace Catalog.Application.Validators;

public sealed class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Isbn).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(80);
        RuleFor(x => x.PublicationYear).InclusiveBetween(1450, 2100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalCopies).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Genre).NotEmpty().MaximumLength(80);
        RuleFor(x => x.PublicationYear).InclusiveBetween(1450, 2100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
