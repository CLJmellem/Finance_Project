using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using TransactionsService.Domain.Entities;
using TransactionsService.Infrastructure.Persistence;

namespace TransactionsService.Infrastructure;

/// <summary>
/// Helth check for MongoDB connectivity.
/// </summary>
/// <seealso cref="Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck" />
public sealed class MongoDbHealthCheck(MongoDbContext<TransactionsDataEntity> context) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext hcContext, CancellationToken ct = default)
    {
        try
        {
            // The ping command is a simple way to check if the MongoDB server is responsive.
            await context.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1), cancellationToken: ct);

            return HealthCheckResult.Healthy("MongoDB connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "MongoDB connection failed.", exception: ex);
        }
    }
}