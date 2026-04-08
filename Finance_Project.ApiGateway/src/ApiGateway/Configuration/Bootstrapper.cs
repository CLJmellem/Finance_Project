using ApiGateway.HealthChecks;
using Polly;
using Polly.CircuitBreaker;
using Yarp.ReverseProxy.Forwarder;

namespace ApiGateway.Configuration;

/// <summary>
/// Registro de dependências do API Gateway.
/// </summary>
public static class Bootstrapper
{
    private static readonly string[] ClusterNames =
        ["cards-cluster", "transactions-cluster", "auth-cluster"];

    /// <summary>
    /// Registra serviços de infraestrutura: YARP, Polly Circuit Breaker, health checks, OpenApi.
    /// </summary>
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // YARP Reverse Proxy
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        // Circuit Breaker (Polly v8) — um pipeline por cluster
        RegisterCircuitBreakers(services, configuration);

        // Factory customizada que integra Polly com YARP
        services.AddSingleton<IForwarderHttpClientFactory, ResilientForwarderHttpClientFactory>();

        // Health checks dos serviços downstream
        services.AddHttpClient("health-check-client")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        services.AddHealthChecks()
            .AddCheck<DownstreamHealthCheck>("downstream-services");

        services.AddControllers();
        services.AddOpenApi();
    }

    /// <summary>
    /// Registra repositórios — não aplicável ao Gateway.
    /// </summary>
    public static void RegisterRepositories(IServiceCollection services)
    {
        // Gateway não possui repositórios
    }

    /// <summary>
    /// Registra validators — não aplicável ao Gateway.
    /// </summary>
    public static void RegisterValidators(IServiceCollection services)
    {
        // Gateway não possui validators
    }

    /// <summary>
    /// Registra um ResiliencePipeline (Circuit Breaker) por cluster YARP.
    /// </summary>
    private static void RegisterCircuitBreakers(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CircuitBreakerSettings.SectionName)
            .Get<CircuitBreakerSettings>() ?? new CircuitBreakerSettings();

        foreach (var clusterName in ClusterNames)
        {
            services.AddResiliencePipeline<string, HttpResponseMessage>(clusterName, (builder, context) =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                    .CreateLogger($"CircuitBreaker.{clusterName}");

                builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    Name = $"cb-{clusterName}",
                    FailureRatio = settings.FailureRatio,
                    SamplingDuration = TimeSpan.FromSeconds(settings.SamplingDurationSeconds),
                    MinimumThroughput = settings.MinimumThroughput,
                    BreakDuration = TimeSpan.FromSeconds(settings.BreakDurationSeconds),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .HandleResult(r => (int)r.StatusCode >= 500)
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>(),
                    OnOpened = args =>
                    {
                        logger.LogWarning(
                            "Circuit Breaker OPENED for '{Cluster}'. Break duration: {Duration}s. Reason: {Outcome}",
                            clusterName, settings.BreakDurationSeconds, args.Outcome.Exception?.Message ?? "Server error");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        logger.LogInformation(
                            "Circuit Breaker CLOSED for '{Cluster}'. Service recovered.",
                            clusterName);
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = args =>
                    {
                        logger.LogInformation(
                            "Circuit Breaker HALF-OPEN for '{Cluster}'. Allowing probe request.",
                            clusterName);
                        return ValueTask.CompletedTask;
                    }
                });
            });
        }
    }
}
