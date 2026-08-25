using AcademiaDoZe.Infrastructure.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Data;

public enum DatabaseType
{
    SqlServer,
    MySql,
    Sqlite
}
public static class DbProvider
{
    public const int DefaultCommandTimeout = 30; // segundos

                                             // IR INCLUINDO OS MÉTODOS AQUI


public static DbConnection CreateConnection(string connectionString, DatabaseType dbType)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InfrastructureException("CONEXAO_STRING_VAZIA", "String de conexão não pode ser vazia."); try
        {
            DbConnection connection = dbType switch
            {
                DatabaseType.SqlServer => new SqlConnection(connectionString),
                DatabaseType.MySql => new MySqlConnection(connectionString),
                DatabaseType.Sqlite => new SqliteConnection(connectionString),
                _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
            };
            if (connection == null) throw new InfrastructureException("FALHA_CONEXAO", $"Falha ao instanciar conexão para {dbType}.");
            return connection;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("FALHA_CONEXAO", $"Falha ao abrir conexão para {dbType}.", ex);
        }
    }
    public static DbCommand CreateCommand(string commandText, DbConnection connection, CommandType commandType = CommandType.Text)
    {
        if (connection == null) throw new InfrastructureException("CONEXAO_NULA", "Conexão não pode ser nula para criar um comando.");
        if (string.IsNullOrWhiteSpace(commandText)) throw new InfrastructureException("COMANDO_TEXTO_VAZIO", "Texto do comando não pode ser vazio."); try
        {
            var command = connection.CreateCommand() ?? throw new InfrastructureException("FALHA_CRIAR_COMANDO", "Falha ao criar o comando no banco de dados."); command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = DefaultCommandTimeout;
            return command;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("FALHA_CRIAR_COMANDO", "Falha ao criar o comando no banco de dados.", ex);
        }
    }

    public static DbParameter AddParameter(this DbCommand command, string name, object? value, DbType dbType)
    {
        if (command == null) throw new InfrastructureException("COMANDO_NULO", "Comando não pode ser nulo para criar parâmetro.");
        if (string.IsNullOrWhiteSpace(name)) throw new InfrastructureException("PARAMETRO_NOME_VAZIO", "Nome do parâmetro não pode ser vazio."); try
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            parameter.DbType = dbType;
            command.Parameters.Add(parameter);
            return parameter;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_CRIAR_PARAMETRO", "Erro ao criar parâmetro no banco de dados.", ex);
        }
    }
    public static async Task<int> ExecuteScalarIdAsync(this DbCommand command, string errorCode = "ERRO_OBTER_ID", string errorMessage = "Falha ao obter ID inserido no banco de dados.", CancellationToken cancellationToken = default)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result != null && result != DBNull.Value)
        {
            return Convert.ToInt32(result);
        }
        throw new InfrastructureException(errorCode, errorMessage);
    }

    #region Funções ajustadas conforme o SGDB selecionado
    public static string GetScriptName(DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => "script_sqlserver.sql",
            DatabaseType.MySql => "script_mysql.sql",
            DatabaseType.Sqlite => "script_sqlite.sql",
            _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
        };
    }
    public static string FormatInsertQuery(string insertSql, DatabaseType dbType)
    {
        if (string.IsNullOrWhiteSpace(insertSql)) throw new InfrastructureException("SQL_INSERT_VAZIO", "Comando SQL de INSERT não pode ser vazio.");
        return dbType switch
        {
            DatabaseType.SqlServer => $"{insertSql}; SELECT SCOPE_IDENTITY();",
            DatabaseType.MySql => $"{insertSql}; SELECT LAST_INSERT_ID();",
            DatabaseType.Sqlite => $"{insertSql}; SELECT last_insert_rowid();",
            _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
        };
    }
    public static string GetCurrentDateFunction(DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => "GETDATE()",
            DatabaseType.MySql => "CURRENT_DATE()",
            DatabaseType.Sqlite => "DATE('now')",
            _ => "CURRENT_DATE()"
        };
    }
    public static string GetDateAddDaysExpression(string dateExpr, string daysParam, DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => $"DATEADD(day, {daysParam}, {dateExpr})",
            DatabaseType.MySql => $"DATE_ADD({dateExpr}, INTERVAL {daysParam} DAY)",
            DatabaseType.Sqlite => $"DATE({dateExpr}, '+' || {daysParam} || ' days')",
            _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
        };
    }
    public static string GetDateHourExpression(string dateColumn, DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => $"DATEPART(HOUR, {dateColumn})",
            DatabaseType.MySql => $"HOUR({dateColumn})",
            DatabaseType.Sqlite => $"CAST(strftime('%H', {dateColumn}) AS INTEGER)",
            _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
        };
    }
    public static string GetDateMonthExpression(string dateColumn, DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => $"MONTH({dateColumn})",
            DatabaseType.MySql => $"MONTH({dateColumn})",
            DatabaseType.Sqlite => $"CAST(strftime('%m', {dateColumn}) AS INTEGER)",
            _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
        };
    }
    public static string GetDateDayExpression(string dateColumn, DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => $"DAY({dateColumn})",
            DatabaseType.MySql => $"DAY({dateColumn})",
            DatabaseType.Sqlite => $"CAST(strftime('%d', {dateColumn}) AS INTEGER)",
            _ => throw new InfrastructureException("SGDB_NAO_SUPORTADO", $"SGDB não suportado: {dbType}")
        };
    }
}
#endregion