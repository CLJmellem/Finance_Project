using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TransactionsService.Domain.Entities.Interface;
using TransactionsService.Domain.Enums;

namespace TransactionsService.Domain.Entities;

/// <summary>
/// TransactionsDataEntity
/// </summary>
/// <seealso cref="TransactionsService.Domain.Entities.Interface.IBaseEntity" />
public class TransactionsDataEntity : IBaseEntity
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    /// <summary>Gets or sets the user identifier.</summary>
    /// <value>The user identifier.</value>
    public ObjectId UserId { get; set; }

    /// <summary>Gets or sets the card identifier.</summary>
    /// <value>The card identifier.</value>
    public ObjectId CardId { get; set; }

    /// <summary>Gets or sets the total amount.</summary>
    /// <value>The total amount.</value>
    [BsonRepresentation(BsonType.Decimal128)]
    public double TotalAmount { get; set; }

    /// <summary>Gets or sets the month amount.</summary>
    /// <value>The month amount.</value>
    [BsonRepresentation(BsonType.Decimal128)]
    public double? MonthAmount { get; set; }

    /// <summary>Gets or sets the installments.</summary>
    /// <value>The installments.</value>
    public int? Installments { get; set; }

    /// <summary>Gets or sets the actual installments.</summary>
    /// <value>The actual installments.</value>
    public int? ActualInstallments { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The type.</value>
    [BsonRepresentation(BsonType.String)]
    public TransactionType Type { get; set; }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the card due date.</summary>
    /// <value>The card due date.</value>
    public DateTime CardDueDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is recurring.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is recurring; otherwise, <c>false</c>.
    /// </value>
    public bool IsRecurring { get; set; } = false;

    /// <summary>Gets the created at.</summary>
    /// <value>The created at.</value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the last updated.</summary>
    /// <value>The last updated.</value>
    public DateTime? LastUpdated { get; set; }
}
