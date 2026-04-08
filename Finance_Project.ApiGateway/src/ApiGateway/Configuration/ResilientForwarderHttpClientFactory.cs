using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Registry;
using Yarp.ReverseProxy.Forwarder;

namespace ApiGateway.Configuration;

/// <summary>
/// Factory customizada que envolve o HttpMessageHandler do YARP com o Circuit Breaker do Polly v8.
/// Cada cluster recebe seu próprio ResiliencePipeline independente.
/// </summary>
public sealed class ResilientForwarderHttpClientFactory : ForwarderHttpClientFactory
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public ResilientForwarderHttpClientFactory(
        ILogger<ForwarderHttpClientFactory> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
        : base(logger)
    {
        _pipelineProvider = pipelineProvider;
    }

    protected override HttpMessageHandler WrapHandler(
        ForwarderHttpClientContext context,
        HttpMessageHandler handler)
    {
        var wrappedHandler = base.WrapHandler(context, handler);

        if (_pipelineProvider.TryGetPipeline<HttpResponseMessage>(
                context.ClusterId, out var pipeline))
        {
            return new ResilienceHandler(pipeline)
            {
                InnerHandler = wrappedHandler
            };
        }

        return wrappedHandler;
    }
}
