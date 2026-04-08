using Auth.Domain.Entities.Interface;
using MongoDB.Bson;

namespace Auth.Domain.Entities
{
    /// <summary>
    /// TokenDataEntity
    /// </summary>
    /// <seealso cref="Auth.Domain.Entities.Interface.IBaseEntity" />
    public class TokenDataEntity : IBaseEntity
    {
        /// <summary>Gets or sets the user identifier.</summary>
        /// <value>The user identifier.</value>
        public required ObjectId UserId { get; set; }

        /// <summary>Gets or sets the refresh token.</summary>
        /// <value>The refresh token.</value>
        public required string RefreshToken { get; set; }

        /// <summary>Gets or sets the refresh token expiration.</summary>
        /// <value>The refresh token expiration.</value>
        public DateTime RefreshTokenExpireAt { get; set; }
    }
}