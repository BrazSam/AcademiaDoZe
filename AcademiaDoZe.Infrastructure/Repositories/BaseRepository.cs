using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;
/*
BaseRepository é uma classe utilitária de infraestrutura focada em gerenciamento de conexões (GetOpenConnectionAsync e Dispose/DisposeAsync).
As operações de CRUD e mapeamento são implementadas diretamente nas classes filhas (AlunoRepository, ColaboradorRepository, etc.).
*/
public abstract class BaseRepository : IDisposable, IAsyncDisposable
{
    protected readonly string _connectionString;
    protected readonly DatabaseType _databaseType;
    private DbConnection? _connection;
    private bool _disposed;
    protected BaseRepository(string connectionString, DatabaseType databaseType)
    {
        _connectionString = connectionString ?? throw new InfrastructureException("STRING_CONEXAO_NULA", $"String de conexão não pode ser nula: {nameof(connectionString)}"); _databaseType = databaseType;
    }
    // IR INCLUINDO OS MÉTODOS AQUI

protected virtual async Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        try
        {
            // cria o banco e as tabelas do banco de dados se não existir
            await DbInitializer.InicializarAsync(_connectionString, _databaseType, cancellationToken);
            if (_connection == null)
            {
                _connection = DbProvider.CreateConnection(_connectionString, _databaseType);
                await _connection.OpenAsync(cancellationToken);
            }
            else if (_connection.State == ConnectionState.Broken)
            {
                await _connection.CloseAsync();
                await _connection.OpenAsync(cancellationToken);
            }
            else if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync(cancellationToken);
            }
            return _connection;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("FALHA_ABRIR_CONEXAO", "Falha ao abrir conexão com o banco de dados.", ex);
        }
    }
    protected virtual async Task<DbCommand> CreateCommandAsync(string commandText, CancellationToken cancellationToken = default)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        return DbProvider.CreateCommand(commandText, connection);
    }
    protected string FormatInsertQuery(string insertSql) => DbProvider.FormatInsertQuery(insertSql, _databaseType);
    protected string GetCurrentDateFunction() => DbProvider.GetCurrentDateFunction(_databaseType);
    protected string GetDateAddDaysExpression(string dateExpr, string daysParam) => DbProvider.GetDateAddDaysExpression(dateExpr, daysParam, _databaseType); protected string GetDateHourExpression(string dateColumn) => DbProvider.GetDateHourExpression(dateColumn, _databaseType);
    protected string GetDateMonthExpression(string dateColumn) => DbProvider.GetDateMonthExpression(dateColumn, _databaseType);
    protected string GetDateDayExpression(string dateColumn) => DbProvider.GetDateDayExpression(dateColumn, _databaseType);
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose();
                _connection = null;
            }
            _disposed = true;
        }
    }
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
    }
}