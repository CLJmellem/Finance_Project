using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TransactionsService.Domain.Entities.Interface;

namespace TransactionsService.Domain.Entities;

/// <summary>
/// Represents a card document from the shared usersCards collection.
/// Used by the Transactions service to validate card ownership and credit limits.
/// </summary>
public sealed class CardDataEntity : IBaseEntity
{
    [BsonId]
    public ObjectId Id { get; set; }

    public ObjectId UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LastFourDigits { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CreditLimit { get; set; }

    public string Brand { get; set; } = string.Empty;

    public int DueDay { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
