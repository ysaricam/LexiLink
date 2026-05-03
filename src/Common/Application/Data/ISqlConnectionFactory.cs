using System.Data;

namespace LexiLink.Common.Application.Data;

public interface ISqlConnectionFactory
{
    IDbConnection GetOpenConnection();
    IDbConnection CreateNewConnection();

    string GetConnectionString();
}