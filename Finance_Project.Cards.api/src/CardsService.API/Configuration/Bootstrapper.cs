using CardsService.API.Services;
using CardsService.Application.Behaviors;
using CardsService.Application.Commands.CreateCard;
using CardsService.Application.Interfaces;
using CardsService.Application.Mappers;
using CardsService.Infrastructure;
using CardsService.Infrastructure.Configuration;
using CardsService.Infrastructure.Persistence;
using CardsService.Infrastructure.Repositories;
using FluentValidation;
using MediatR;

namespace CardsService.API.Configuration;

/// <summary>
/// Register dependencies for the application.
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Register infrastructure, application, and API services.
    /// </summary>
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton(typeof(MongoDbContext<>));

        var applicationAssembly = typeof(CreateCardCommand).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile(typeof(CardProfile));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>("mongodb");

        services.AddControllers();
        services.AddOpenApi();
    }

    /// <summary>
    /// Register infrastructure layer repositories.
    /// </summary>
    public static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<ICardRepository, CardRepository>();
    }

    /// <summary>
    /// Register FluentValidation validators.
    /// </summary>
    public static void RegisterValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(CreateCardCommand).Assembly);
    }
}
