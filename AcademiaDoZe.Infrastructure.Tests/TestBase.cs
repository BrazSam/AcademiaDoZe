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
            DatabaseType.Sqlite => $"Data Source=C:\\Users\\samue\\Downloads\\UNIPLAC\\FASE 4\\DEV. SISTEMAS II\\AcademiaDoZe\\db_academia_do_ze.db;Cache=Shared;",
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };
    }
    #region Geradores de dados aleatórios
    private static int _counter = 10000;
    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + 
    Interlocked.Increment(ref _counter)).ToString("D8")[..8];
    protected static string GerarCpf()
    {
        // Gera 9 dígitos base aleatórios (evitando todos iguais)
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
        int[] baseCpf = new int[9];
        do
        {
            for (int i = 0; i < 9; i++) baseCpf[i] = rnd.Next(0, 10);
        } while (baseCpf.Distinct().Count() == 1);

        // 1º dígito verificador
        int soma = 0;
        for (int i = 0; i < 9; i++) soma += baseCpf[i] * (10 - i);
        int resto = soma % 11;
        int dv1 = resto < 2 ? 0 : 11 - resto;

        // 2º dígito verificador
        soma = 0;
        for (int i = 0; i < 9; i++) soma += baseCpf[i] * (11 - i);
        soma += dv1 * 2;
        resto = soma % 11;
        int dv2 = resto < 2 ? 0 : 11 - resto;

        return string.Concat(baseCpf) + dv1 + dv2;
    }


    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";


    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];
    #endregion

    protected string ObterSiglaSgbd() => DatabaseType switch
    {
        DatabaseType.Sqlite => "SQLite",
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        _ => throw new ArgumentOutOfRangeException()
    };
}