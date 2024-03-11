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

        services.AddScoped<VersaDatabaseService>();
        services.AddScoped<MetadataReader>();

        return services;
    }

    public static IApplicationBuilder UseVersa(this WebApplication applicationBuilder, VersaOptions options)
    {
        using var scope = applicationBuilder.Services.CreateScope();
        var databaseService = scope.ServiceProvider.GetRequiredService<VersaDatabaseService>();
        databaseService.EnsureDatabaseCreated();

        if (options.ReadMetadataOnStartup)
        {
            var metadataReader = scope.ServiceProvider.GetRequiredService<MetadataReader>();
            var metadata = metadataReader.ReadTargetDatabaseMetadata(options.TargetDbConnectionString);
            databaseService.UpdateSavedMetadata(metadata);
        }

        return applicationBuilder;
    }
}

public class VersaOptions
{
    public bool ReadMetadataOnStartup { get; set; }
    public string TargetDbConnectionString { get; set; }
}

public class VersaDbConfiguration
{
    public string ConnectionString { get; set; }
}
