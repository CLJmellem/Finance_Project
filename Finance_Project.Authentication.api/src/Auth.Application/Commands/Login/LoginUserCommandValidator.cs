using Auth.Application.Bases;
using Auth.Application.Constants;
using Auth.Domain.Command.Login;
using FluentValidation;

namespace Auth.Application.Commands.Login
{
    /// <summary>
    /// LoginUserCommandValidator
    /// </summary>
    /// <seealso cref="Auth.Application.Bases.BaseValidators&lt;Auth.Domain.Command.Login.LoginUserCommand&gt;" />
    public class LoginUserCommandValidator : BaseValidators<LoginUserCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginUserCommandValidator"/> class.
        /// </summary>
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage(ValidationMessages.RequiredUsernameOrEmail);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ValidationMessages.RequiredPassword);
        }
    }
}