using FluentValidation;
using TransactionsService.Application.Constants;
using TransactionsService.Application.Validators;

namespace TransactionsService.Application.Commands.UpdateTransaction;

/// <summary>
/// Validator for UpdateTransactionCommand.
/// </summary>
public sealed class UpdateTransactionCommandValidator : BaseValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
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

        When(x => x.TotalAmount.HasValue, () =>
        {
            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage(ValidationMessages.InvalidTotalAmount);
        });

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

        When(x => x.Type.HasValue, () =>
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage(ValidationMessages.InvalidTransactionType);
        });

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage(ValidationMessages.MaxLengthDescription)
            .When(x => x.Description is not null);
    }
}
