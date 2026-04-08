using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;

namespace Auth.Infrastructure.Repositories
{
    /// <summary>
    /// TokenRepository
    /// </summary>
    /// <seealso cref="Auth.Infrastructure.Repositories.BaseRepository&lt;Auth.Domain.Entities.TokenDataEntity&gt;" />
    /// <seealso cref="Auth.Domain.Interfaces.ITokenRepository" />
    public class TokenRepository(MongoDbContext<TokenDataEntity> context)
        : BaseRepository<TokenDataEntity>("tokenRegister", context), ITokenRepository
    {
    }
}