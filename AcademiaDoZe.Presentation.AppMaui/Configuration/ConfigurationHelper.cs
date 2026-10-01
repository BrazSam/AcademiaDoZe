using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // 1. Tipo de banco de dados: SqlServer, MySql ou Sqlite
        var databaseType = AppDatabaseType.Sqlite;
        // 2. Configuração da Connection String de acordo com o banco escolhido
        string connectionString;
        if (databaseType == AppDatabaseType.Sqlite)
        {
            var dbPath = DeviceInfo.Platform == DevicePlatform.WinUI
            ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
            : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
            connectionString = $"Data Source={dbPath};Default Timeout=5;";
        }
        else
        {
            const string dbServer = "10.30.21.16";
            const string dbDatabase = "db_academia_do_ze";
            const string dbUser = "root";
            const string dbPassword = "abcBolinhas12345";
            // ajuste do complemento de acordo com o tipo de banco de dados escolhido
            string dbComplemento = string.Empty;
            if (databaseType == AppDatabaseType.SqlServer)
            {
                dbComplemento = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";
            }
            else if (databaseType == AppDatabaseType.MySql)
            {
                dbComplemento = "Connection Timeout=5;Default Command Timeout=30;";
            }
            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }
        // 3. Configura a fábrica de repositórios com a string de conexão e tipo de banco
        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        });
        // 4. Configura os serviços da camada de aplicação
        services.AddApplicationServices();
    }
}