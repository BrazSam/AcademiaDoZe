using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Data;

public static class DataReaderExtensions //Serve para facilitar a leitura de valores de colunas em um DbDataReader,
                                         //fornecendo métodos de extensão para obter valores de forma mais segura e conveniente.
{
    public static string GetStringValue(this DbDataReader reader, string columnName)
    {
        return reader[columnName].ToString()!;
    }
    public static string GetNullableString(this DbDataReader reader, string columnName, string defaultValue = "")
    {
        return reader[columnName] is DBNull ? defaultValue : reader[columnName].ToString()!;
    }
    public static byte[]? GetNullableBytes(this DbDataReader reader, string columnName)
    {
        return reader[columnName] is DBNull ? null : (byte[])reader[columnName];
    }
    public static int GetInt32Value(this DbDataReader reader, string columnName)
    {
        return Convert.ToInt32(reader[columnName]);
    }
    public static DateTime GetDateTimeValue(this DbDataReader reader, string columnName)
    {
        return Convert.ToDateTime(reader[columnName]);
    }
    public static DateOnly GetDateOnlyValue(this DbDataReader reader, string columnName)
    {
        return DateOnly.FromDateTime(Convert.ToDateTime(reader[columnName]));
    }
}