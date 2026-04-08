using Auth.Application.Bases;
using Auth.Application.Constants;
using Auth.Domain.Command.Logout;
using FluentValidation;

namespace Auth.Application.Commands.Logout
{
    /// <summary>
    /// LogoutUserCommandValidator
    /// </summary>
    /// <seealso cref="Auth.Application.Bases.BaseValidators&lt;Auth.Domain.Command.Logout.LogoutUserCommand&gt;" />
    public class LogoutUserCommandValidator : BaseValidators<LogoutUserCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutUserCommandValidator"/> class.
        /// </summary>
        public LogoutUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(ValidationMessages.RequiredUserId)
                .Must(value => BeAValidObjectId(value)).WithMessage(ValidationMessages.InvalidUserId);
        }
    }
}
