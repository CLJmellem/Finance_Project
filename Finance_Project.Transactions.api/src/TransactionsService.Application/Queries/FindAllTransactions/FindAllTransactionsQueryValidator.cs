using FluentValidation;
using TransactionsService.Application.Constants;
using TransactionsService.Application.Validators;

namespace TransactionsService.Application.Queries.FindAllTransactions;

/// <summary>
/// Validates optional query parameters for FindAllTransactionsQuery.
/// All fields are optional — only validates format when a value is provided.
/// </summary>
public sealed class FindAllTransactionsQueryValidator : BaseValidator<FindAllTransactionsQuery>
{
    public FindAllTransactionsQueryValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.UserId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.UserId));
        });

        When(x => !string.IsNullOrWhiteSpace(x.CardId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.CardId));
        });

        When(x => !string.IsNullOrWhiteSpace(x.TransactionId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.TransactionId));
        });

        When(x => !string.IsNullOrWhiteSpace(x.LastFourDigits), () =>
        {
            RuleFor(x => x.LastFourDigits)
                .Matches(@"^\d{4}$")
                .WithMessage(ValidationMessages.InvalidLastFourDigitsFormat);
        });
    }
}
