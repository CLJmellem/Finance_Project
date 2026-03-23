using MongoDB.Bson;

namespace TransactionsService.Domain.Entities.Interface;

/// <summary>
/// IBaseEntity
/// </summary>
public interface IBaseEntity
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    ObjectId Id { get; }

    /// <summary>Gets the created at.</summary>
    /// <value>The created at.</value>
    DateTime CreatedAt { get; }
}
