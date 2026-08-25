using AcademiaDoZe.Infrastructure.Exceptions;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;
namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, bool> _bancosInicializados = new();
    public static async Task InicializarAsync(string connectionString, DatabaseType databaseType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;
        var key = $"{databaseType}:{connectionString}";
        if (_bancosInicializados.ContainsKey(key)) return;
        var scriptSql = ObterScript(databaseType);
        try
        {
            await using var connection = DbProvider.CreateConnection(connectionString, databaseType);
            await connection.OpenAsync(cancellationToken);
            await using var command = DbProvider.CreateCommand(scriptSql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
            _bancosInicializados.TryAdd(key, true);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_INICIALIZAR_BANCO", $"Erro ao inicializar banco de dados: {ex.Message}", ex);
        }
    }
    public static string ObterScript(DatabaseType databaseType)
    {
        var nomeScript = DbProvider.GetScriptName(databaseType);
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
        .FirstOrDefault(r => r.EndsWith(nomeScript, StringComparison.OrdinalIgnoreCase))
        ?? throw new InfrastructureException("SCRIPT_EMBARCADO_NAO_ENCONTRADO", $"Script SQL embarcado '{nomeScript}' não encontrado.");
        using var stream = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InfrastructureException("ERRO_LEITURA_SCRIPT", $"Erro ao carregar o fluxo do script embarcado '{nomeScript}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}