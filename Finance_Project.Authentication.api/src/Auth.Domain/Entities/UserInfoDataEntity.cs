using Auth.Domain.Entities.Interface;
using Auth.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Auth.Domain.Entities;

/// <summary>
/// UserInfoDataEntity
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="UserInfoDataEntity"/> class.</remarks>
/// <param name="username">The username.</param>
/// <param name="email">The email.</param>
/// <param name="passwordHash">The password hash.</param>
public class UserInfoDataEntity(string username, string email, string passwordHash) : IBaseEntity
{
    /// <summary>Gets or sets the username.</summary>
    /// <value>The username.</value>
    [BsonElement("username")]
    [BsonRequired]
    public string Username { get; set; } = username;

    /// <summary>Gets or sets the email.</summary>
    /// <value>The email.</value>
    [BsonElement("email")]
    [BsonRequired]
    public string Email { get; set; } = email;

    /// <summary>Gets or sets the password hash.</summary>
    /// <value>The password hash.</value>
    [BsonElement("passwordHash")]
    [BsonRequired]
    public string PasswordHash { get; set; } = passwordHash;

    /// <summary>Gets or sets the role.</summary>
    /// <value>The role.</value>
    [BsonElement("role")]
    public UserRolesEnum Role { get; set; } = UserRolesEnum.FreeUser;

    /// <summary>Gets or sets the created at.</summary>
    /// <value>The created at.</value>
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the updated at.</summary>
    /// <value>The updated at.</value>
    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is email confirmed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is email confirmed; otherwise, <c>false</c>.
    /// </value>
    [BsonElement("isEmailConfirmed")]
    public bool IsEmailConfirmed { get; set; } = false;
}