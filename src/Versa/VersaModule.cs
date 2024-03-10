using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Versa.Database;
using Versa.Database.Metadata;

namespace Versa;

public static class VersaModule
{
    public static IServiceCollection AddVersa(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(new VersaDbConfiguration { ConnectionString = connectionString });

        services.AddScoped<DatabaseCreator>();
        services.AddScoped<MetadataReader>();

        return services;
    }

    public static IApplicationBuilder UseVersa(this WebApplication applicationBuilder)
    {
        using var scope = applicationBuilder.Services.CreateScope();
        var databaseCreator = scope.ServiceProvider.GetRequiredService<DatabaseCreator>();
        databaseCreator.EnsureDatabaseCreated();

        return applicationBuilder;
    }
}

public class VersaDbConfiguration
{
    public string ConnectionString { get; set; }
}
