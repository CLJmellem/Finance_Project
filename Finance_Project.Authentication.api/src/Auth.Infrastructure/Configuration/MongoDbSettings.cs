namespace Auth.Infrastructure.Configuration;

/// <summary>
/// MongoDB settings.
/// </summary>
public class MongoDbSettings
{
    /// <summary>The section name</summary>
    public const string SectionName = "MongoDbSettings";

    /// <summary>Gets or sets the connection string.</summary>
    /// <value>The connection string.</value>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the database.</summary>
    /// <value>The name of the database.</value>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the collection.</summary>
    /// <value>The name of the collection.</value>
    public string CollectionName { get; set; } = string.Empty;
}