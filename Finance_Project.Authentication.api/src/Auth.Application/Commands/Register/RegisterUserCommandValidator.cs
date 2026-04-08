using Auth.Application.Bases;
using Auth.Application.Constants;
using Auth.Domain.Commands.Register;
using FluentValidation;

namespace Auth.Application.Commands.Register;

/// <summary>
/// RegisterUserCommandValidator
/// </summary>
/// <seealso cref="Auth.Application.Bases.BaseValidators&lt;Auth.Domain.Commands.Register.RegisterUserCommand&gt;" />
public class RegisterUserCommandValidator : BaseValidators<RegisterUserCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterUserCommandValidator"/> class.
    /// </summary>
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationMessages.RequiredUsername)
            .MinimumLength(3).WithMessage(ValidationMessages.MinLengthUsername)
            .MaximumLength(20).WithMessage(ValidationMessages.MaxLengthUsername)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage(ValidationMessages.InvalidFormatUsername);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.RequiredEmail)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(100).WithMessage(ValidationMessages.MaxLengthEmail);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.RequiredPassword)
            .MinimumLength(8).WithMessage(ValidationMessages.MinLengthPassword)
            .Matches("[A-Z]").WithMessage(ValidationMessages.RequiresUppercasePassword)
            .Matches("[a-z]").WithMessage(ValidationMessages.RequiresLowercasePassword)
            .Matches("[0-9]").WithMessage(ValidationMessages.RequiresDigitPassword)
            .Matches("[^a-zA-Z0-9]").WithMessage(ValidationMessages.RequiresSpecialCharPassword);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(ValidationMessages.RequiredConfirmPassword)
            .DependentRules(() => {
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(ValidationMessages.PasswordNotMatche);
            });
    }
}