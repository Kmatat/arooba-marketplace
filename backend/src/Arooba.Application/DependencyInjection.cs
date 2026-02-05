using System.Reflection;
using Arooba.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Arooba.Application;

/// <summary>
/// Registers all Application-layer services (MediatR, FluentValidation, AutoMapper)
/// into the Microsoft DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application-layer services including MediatR handlers, FluentValidation
    /// validators, AutoMapper profiles, and pipeline behaviors.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
