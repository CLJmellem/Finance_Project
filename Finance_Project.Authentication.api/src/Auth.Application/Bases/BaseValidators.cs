using FluentValidation;
using MongoDB.Bson;

namespace Auth.Application.Bases
{
    /// <summary>
    /// BaseValidators
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="FluentValidation.AbstractValidator&lt;T&gt;" />
    public abstract class BaseValidators<T> : AbstractValidator<T>
    {
        /// <summary>Bes a valid object identifier.</summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool BeAValidObjectId(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && ObjectId.TryParse(value, out _);
        }
    }
}