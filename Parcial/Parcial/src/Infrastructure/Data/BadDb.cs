using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Infrastructure.Data;

public static class BadDb
{
    // No hardcodear: leer desde variable de entorno (o IConfiguration/KeyVault)
    private static readonly string ConnectionString = GetConnectionString();

    private static string GetConnectionString()
    {
        var cs = Environment.GetEnvironmentVariable("DB_CONNECTIONSTRING");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Connection string not configured. Set DB_CONNECTIONSTRING.");
        return cs;
    }

    // Método seguro: usa using, acepta parámetros (evita inyección SQL)
    public static int ExecuteNonQuery(string sql, IEnumerable<SqlParameter>? parameters = null)
    {
        using var conn = new SqlConnection(ConnectionString);
        using var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
            }
        }

        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    // Reader seguro: devuelve IDataReader y cierra la conexión al disponer el reader
    public static IDataReader ExecuteReader(string sql, IEnumerable<SqlParameter>? parameters = null)
    {
        var conn = new SqlConnection(ConnectionString);
        var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text };

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                cmd.Parameters.Add(new SqlParameter(p.ParameterName, p.Value ?? DBNull.Value));
            }
        }

        conn.Open();
        return cmd.ExecuteReader(CommandBehavior.CloseConnection);
    }
}
