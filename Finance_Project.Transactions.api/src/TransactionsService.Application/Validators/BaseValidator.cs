using FluentValidation;
using MongoDB.Bson;
using TransactionsService.Application.Constants;

namespace TransactionsService.Application.Validators;

/// <summary>
/// Base validator that provides reusable validation rules shared across all validators,
/// such as ObjectId format checking.
/// </summary>
public abstract class BaseValidator<T> : AbstractValidator<T>
{
    /// <summary>
    /// Applies an ObjectId format rule to the given string property rule builder.
    /// </summary>
    protected IRuleBuilderOptions<T, string?> MustBeValidObjectId(IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage(ValidationMessages.InvalidObjectId);
    }
}
