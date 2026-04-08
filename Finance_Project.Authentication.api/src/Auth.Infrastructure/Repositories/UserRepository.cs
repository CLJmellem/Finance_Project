using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Persistence;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// UserRepository
/// </summary>
/// <seealso cref="Auth.Infrastructure.Repositories.BaseRepository&lt;Auth.Domain.Entities.UserInfoDataEntity&gt;" />
/// <seealso cref="Auth.Domain.Interfaces.IUserRepository" />
public class UserRepository(MongoDbContext<UserInfoDataEntity> context) : BaseRepository<UserInfoDataEntity>("usersInfo", context), IUserRepository
{
    /// <summary>Gets the by email asynchronous.</summary>
    /// <param name="email">The email.</param>
    /// <returns></returns>
    public async Task<UserInfoDataEntity?> GetByEmailAsync(string email)
    {
        return (await GetAllAsync(u => u.Email == email)).FirstOrDefault();
    }

    /// <summary>Gets the by username asynchronous.</summary>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    public async Task<UserInfoDataEntity?> GetByUsernameAsync(string username)
    {
        return (await GetAllAsync(u => u.Username == username)).FirstOrDefault();
    }
}