using CardsService.Domain.Entities.Interface;
using CardsService.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CardsService.Domain.Entities;

/// <summary>
/// CardDataEntity
/// </summary>
/// <seealso cref="CardsService.Domain.Entities.Interface.IBaseEntity" />
public class CardDataEntity : IBaseEntity
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>Gets or sets the user identifier.</summary>
    /// <value>The user identifier.</value>
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the name.</summary>
    /// <value>The name.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the brand.</summary>
    /// <value>The brand.</value>
    [BsonRepresentation(BsonType.String)]
    public CardBrand Brand { get; set; }

    /// <summary>Gets or sets the last four digits.</summary>
    /// <value>The last four digits.</value>
    public string LastFourDigits { get; set; } = string.Empty;

    /// <summary>Gets or sets the credit limit.</summary>
    /// <value>The credit limit.</value>
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CreditLimit { get; set; }

    /// <summary>Gets or sets the due day.</summary>
    /// <value>The due day.</value>
    public int DueDay { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is active.
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets the created at.</summary>
    /// <value>The created at.</value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the updated at.</summary>
    /// <value>The updated at.</value>
    public DateTime? UpdatedAt { get; set; }
}