using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.HealthChecks;

/// <summary>
/// Health check que verifica a disponibilidade dos serviços downstream.
/// </summary>
public sealed class DownstreamHealthCheck(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<DownstreamHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var clusters = configuration.GetSection("ReverseProxy:Clusters").GetChildren();
        var results = new Dictionary<string, string>();
        var allHealthy = true;
        var anyHealthy = false;

        var client = httpClientFactory.CreateClient("health-check-client");
        client.Timeout = TimeSpan.FromSeconds(5);

        foreach (var cluster in clusters)
        {
            var clusterName = cluster.Key;
            var address = cluster.GetSection("Destinations")
                .GetChildren()
                .FirstOrDefault()?
                .GetValue<string>("Address");

            if (string.IsNullOrEmpty(address))
            {
                results[clusterName] = "No address configured";
                allHealthy = false;
                continue;
            }

            try
            {
                var response = await client.GetAsync($"{address}/health", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    results[clusterName] = "Healthy";
                    anyHealthy = true;
                }
                else
                {
                    results[clusterName] = $"Unhealthy (HTTP {(int)response.StatusCode})";
                    allHealthy = false;
                }
            }
            catch (Exception ex)
            {
                results[clusterName] = $"Unreachable ({ex.GetType().Name})";
                allHealthy = false;
                logger.LogWarning("Health check failed for '{Cluster}': {Message}", clusterName, ex.Message);
            }
        }

        var data = results.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        if (allHealthy)
            return HealthCheckResult.Healthy("All downstream services are healthy.", data);

        if (anyHealthy)
            return HealthCheckResult.Degraded("Some downstream services are unhealthy.", data: data);

        return HealthCheckResult.Unhealthy("All downstream services are unreachable.", data: data);
    }
}
