using CardsService.Application.Constants;
using FluentValidation;

namespace CardsService.Application.Commands.UpdateCard;

/// <summary>
/// UpdateCardCommandValidator
/// </summary>
/// <seealso cref="FluentValidation.AbstractValidator&lt;CardsService.Application.Commands.UpdateCard.UpdateCardCommand&gt;" />
public sealed class UpdateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public UpdateCardCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage(ValidationMessages.RequiredCardId);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.RequiredCardName)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLengthCardName);

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.InvalidCreditLimit);

        RuleFor(x => x.DueDay)
            .InclusiveBetween(1, 31).WithMessage(ValidationMessages.InvalidDueDay);
    }
}