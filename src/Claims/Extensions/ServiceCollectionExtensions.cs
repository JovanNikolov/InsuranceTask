using Claims.Data;
using Claims.Repositories;
using Claims.Services;
using Microsoft.EntityFrameworkCore;

namespace Claims.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabases(this IServiceCollection services, string sqlConnectionString, string mongoConnectionString, string mongoDatabaseName)
    {
        services.AddDbContext<AuditContext>(options => options.UseSqlServer(sqlConnectionString));
        services.AddDbContext<ClaimsContext>(options => options.UseMongoDB(mongoConnectionString, mongoDatabaseName));
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClaimsRepository, ClaimsRepository>();
        services.AddScoped<ICoversRepository, CoversRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<CoverValidator>();
        services.AddSingleton<ClaimValidator>();
        services.AddSingleton<PremiumCalculator>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<ICoversService, CoversService>();
        return services;
    }
}
