using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var connectionString =
            "Server=localhost,1433;" +
            "Database=db_academia_do_ze;" +
            "User Id=sa;" +
            "Password=#Bananadepijama123;" +
            "TrustServerCertificate=True;" +
            "Encrypt=False;" +
            "Connect Timeout=10;";

        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = DatabaseType.SqlServer
        });

        services.AddApplicationServices();
    }
}