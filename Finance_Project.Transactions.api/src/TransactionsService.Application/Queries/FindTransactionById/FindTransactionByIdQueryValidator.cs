using FluentValidation;
using TransactionsService.Application.Constants;
using TransactionsService.Application.Validators;

namespace TransactionsService.Application.Queries.FindTransactionById;

/// <summary>
/// Validator for FindTransactionByIdQuery.
/// </summary>
public sealed class FindTransactionByIdQueryValidator : BaseValidator<FindTransactionByIdQuery>
{
    public FindTransactionByIdQueryValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage(ValidationMessages.RequiredCardId);

        When(x => !string.IsNullOrWhiteSpace(x.CardId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.CardId));
        });

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage(ValidationMessages.RequiredTransactionId);

        When(x => !string.IsNullOrWhiteSpace(x.TransactionId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.TransactionId));
        });

        When(x => !string.IsNullOrWhiteSpace(x.UserId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.UserId));
        });
    }
}
