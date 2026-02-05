using Arooba.Application.Common.Interfaces;
using Arooba.Domain.Interfaces;
using Arooba.Infrastructure.Persistence;
using Arooba.Infrastructure.Persistence.Repositories;
using Arooba.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arooba.Infrastructure;

/// <summary>
/// Registers all Infrastructure-layer services with the DI container.
/// Called from the API startup to wire up EF Core, repositories, and service implementations.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core with SQL Server ──
        services.AddDbContext<AroobaDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("AroobaConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AroobaDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                }));

        // ── Interfaces → Implementations ──
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<AroobaDbContext>());

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddTransient<IPricingService, PricingService>();
        services.AddTransient<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
