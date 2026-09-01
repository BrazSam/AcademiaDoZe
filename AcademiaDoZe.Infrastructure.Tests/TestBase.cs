using AcademiaDoZe.Infrastructure.Data;
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]
namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    // Alterne o SGBD alvo dos testes trocando apenas a constante abaixo:
    private const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;
    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }
    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;
        // Ajuste a ConnectionString com caminhos e credenciais válidas
        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=#Bananadepijama123;TrustServerCertificate=True;Encrypt=True;",
            DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=braz;Password=SamuelBraz;",
            DatabaseType.Sqlite => $"Data Source=C:\\Users\\samue\\Downloads\\UNIPLAC\\FASE 4\\DEV. SISTEMAS II\\AcademiaDoze\\db_academia_do_ze2.db;Cache=Shared;",
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };
    }
    #region Geradores de dados aleatórios
    private static int _counter = 10000;
    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + 
    Interlocked.Increment(ref _counter)).ToString("D8")[..8]; protected static string GerarCpf() => (10000000000L + 
    ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    

    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";


    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    #endregion
}