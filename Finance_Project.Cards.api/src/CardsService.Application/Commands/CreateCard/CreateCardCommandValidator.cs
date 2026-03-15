using CardsService.Application.Constants;
using FluentValidation;

namespace CardsService.Application.Commands.CreateCard;

/// <summary>
/// CreateCardCommandValidator
/// </summary>
/// <seealso cref="FluentValidation.AbstractValidator&lt;CardsService.Application.Commands.CreateCard.CreateCardCommand&gt;" />
public sealed class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.RequiredCardName)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLengthCardName);

        RuleFor(x => x.Brand)
            .IsInEnum().WithMessage(ValidationMessages.InvalidCardBrand);

        RuleFor(x => x.LastFourDigits)
            .NotEmpty().WithMessage(ValidationMessages.RequiredLastFourDigits)
            .Length(4).WithMessage(ValidationMessages.ExactLengthLastFourDigits)
            .Matches(@"^\d{4}$").WithMessage(ValidationMessages.InvalidLastFourDigitsFormat);

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.InvalidCreditLimit);

        RuleFor(x => x.DueDay)
            .InclusiveBetween(1, 31).WithMessage(ValidationMessages.InvalidDueDay);
    }
}