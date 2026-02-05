using Arooba.API.Filters;

namespace Arooba.API.Extensions;

/// <summary>
/// Extension methods for registering API-layer services such as controllers,
/// exception filters, and health checks into the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all API-layer services including controllers with the global exception filter,
    /// API behavior configuration, and any API-specific singleton services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilterAttribute>();
        });

        services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddEndpointsApiExplorer();

        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Adds a custom health check endpoint for monitoring the API.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAroobaHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
}
