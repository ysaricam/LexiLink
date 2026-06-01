using System.Data;
using LexiLink.Common.Application.Data;
using Npgsql;

namespace LexiLink.Modules.Ads.Infrastructure.Domain;

internal class SqlConnectionFactory : ISqlConnectionFactory, IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection GetOpenConnection()
    {
        if (_connection is null || _connection.State != ConnectionState.Open)
        {
            _connection = new NpgsqlConnection(_connectionString);
            _connection.Open();
        }

        return _connection;
    }

    public IDbConnection CreateNewConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public string GetConnectionString() => _connectionString;

    public void Dispose()
    {
        if (_connection is { State: ConnectionState.Open })
        {
            _connection.Dispose();
        }
    }
}
