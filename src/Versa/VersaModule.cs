using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.MSSqlServer;
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

        var logger = new LoggerConfiguration()
            .WriteTo
            .MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions { TableName = "InternalLog", AutoCreateSqlTable = true })
            .CreateLogger();

        services.AddSingleton<ILogger>(logger);

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
