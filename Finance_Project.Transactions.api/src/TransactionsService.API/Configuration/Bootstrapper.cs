using FluentValidation;
using MediatR;
using TransactionsService.API.Services;
using TransactionsService.Application.Behaviors;
using TransactionsService.Application.Commands.AddTransaction;
using TransactionsService.Application.Interfaces;
using TransactionsService.Application.Mappers;
using TransactionsService.Infrastructure;
using TransactionsService.Infrastructure.BackgroundServices;
using TransactionsService.Infrastructure.Configuration;
using TransactionsService.Infrastructure.Persistence;
using TransactionsService.Infrastructure.Repositories;

namespace TransactionsService.API.Configuration;

/// <summary>
/// Centralized dependency injection registration for all application layers.
/// </summary>
public static class Bootstrapper
{
    /// <summary>Registers infrastructure, application, and API services.</summary>
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton(typeof(MongoDbContext<>));

        var applicationAssembly = typeof(AddTransactionCommand).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile(typeof(TransactionProfile));
        });

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddHealthChecks()
            .AddCheck<MongoDbHealthCheck>("mongodb");

        // Background service for installment and recurring transaction processing
        services.AddHostedService<InstallmentProcessorService>();

        services.AddControllers();
        services.AddOpenApi();
    }

    /// <summary>Registers infrastructure layer repositories.</summary>
    public static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
    }

    /// <summary>Registers FluentValidation validators.</summary>
    public static void RegisterValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(AddTransactionCommand).Assembly);
    }
}
