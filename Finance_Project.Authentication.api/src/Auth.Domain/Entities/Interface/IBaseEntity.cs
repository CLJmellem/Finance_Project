using MongoDB.Bson;

namespace Auth.Domain.Entities.Interface
{
    /// <summary>
    /// IBaseEntity
    /// </summary>
    public class IBaseEntity
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        public ObjectId Id { get; set; }
    }
}