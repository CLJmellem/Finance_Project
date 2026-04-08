using Auth.Application.Bases;
using Auth.Application.Constants;
using Auth.Domain.Command.Token;
using FluentValidation;

namespace Auth.Application.Commands.Token
{
    /// <summary>
    /// TokenCommandValidator
    /// </summary>
    /// <seealso cref="Auth.Application.Bases.BaseValidators&lt;Auth.Domain.Command.Token.TokenCommand&gt;" />
    public class TokenCommandValidator : BaseValidators<TokenCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCommandValidator"/> class.
        /// </summary>
        public TokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage(ValidationMessages.RequiredRefreshToken);
        }
    }
}