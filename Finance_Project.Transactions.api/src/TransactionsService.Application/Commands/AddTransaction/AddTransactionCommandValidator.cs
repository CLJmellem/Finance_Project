using FluentValidation;
using TransactionsService.Application.Constants;
using TransactionsService.Application.Validators;

namespace TransactionsService.Application.Commands.AddTransaction;

/// <summary>
/// Validator for AddTransactionCommand.
/// </summary>
public sealed class AddTransactionCommandValidator : BaseValidator<AddTransactionCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage(ValidationMessages.RequiredCardId);

        When(x => !string.IsNullOrWhiteSpace(x.CardId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.CardId));
        });

        When(x => !string.IsNullOrWhiteSpace(x.UserId), () =>
        {
            MustBeValidObjectId(RuleFor(x => x.UserId));
        });

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage(ValidationMessages.InvalidTotalAmount);

        // MonthAmount and Installments must be provided together
        When(x => x.MonthAmount.HasValue, () =>
        {
            RuleFor(x => x.MonthAmount)
                .GreaterThan(0).WithMessage(ValidationMessages.InvalidMonthAmount);

            RuleFor(x => x.Installments)
                .NotNull().WithMessage(ValidationMessages.RequiredInstallmentsWhenMonthAmount);
        });

        When(x => x.Installments.HasValue, () =>
        {
            RuleFor(x => x.Installments)
                .GreaterThanOrEqualTo(2).WithMessage(ValidationMessages.InvalidInstallments);

            RuleFor(x => x.MonthAmount)
                .NotNull().WithMessage(ValidationMessages.RequiredMonthAmountWhenInstallments);
        });

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(ValidationMessages.InvalidTransactionType);

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage(ValidationMessages.MaxLengthDescription)
            .When(x => x.Description is not null);
    }
}
