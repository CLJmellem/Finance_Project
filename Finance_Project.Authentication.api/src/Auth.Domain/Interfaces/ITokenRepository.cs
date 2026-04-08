using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces
{
    /// <summary>
    /// ITokenRepository
    /// </summary>
    /// <seealso cref="Auth.Domain.Interfaces.IBaseRepository&lt;Auth.Domain.Entities.TokenDataEntity&gt;" />
    public interface ITokenRepository : IBaseRepository<TokenDataEntity>
    {
    }
}