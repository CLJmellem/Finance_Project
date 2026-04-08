namespace ApiGateway.Configuration;

/// <summary>
/// Configurações do Circuit Breaker via appsettings.json.
/// </summary>
public sealed class CircuitBreakerSettings
{
    public const string SectionName = "CircuitBreaker";

    public double FailureRatio { get; init; } = 0.5;
    public int SamplingDurationSeconds { get; init; } = 30;
    public int MinimumThroughput { get; init; } = 5;
    public int BreakDurationSeconds { get; init; } = 30;
}
