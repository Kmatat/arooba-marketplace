using Arooba.Application.Common.Interfaces;
using Arooba.Infrastructure.Persistence;
using Arooba.Infrastructure.Persistence.Interceptors;
using Arooba.Infrastructure.Persistence.Repositories;
using Arooba.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arooba.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services into the Microsoft DI container.
/// This includes the EF Core DbContext, repository implementations, and all service bindings.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure-layer services including the EF Core DbContext configured for SQL Server,
    /// the generic repository, and all application service implementations.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing connection strings.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Validate the connection string is provided
        var connectionString = configuration.GetConnectionString("AroobaConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:AroobaConnection is not configured. " +
                "Set it via environment variable, user secrets, or appsettings.");
        }

        // Register the auditable entity interceptor
        services.AddScoped<AuditableEntityInterceptor>();

        // Register the EF Core DbContext with SQL Server provider
        services.AddDbContext<AroobaDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();

            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AroobaDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                });

            options.AddInterceptors(interceptor);
        });

        // Register IApplicationDbContext as a scoped service backed by AroobaDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AroobaDbContext>());

        // Register the generic repository for all entity types
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // Register application services
        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddTransient<IPricingService, PricingService>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
