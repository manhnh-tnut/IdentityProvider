using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SSO.Domain.Interfaces;

namespace SSO.Infrastructure.Data.Repositories;

public class QueryRepository : IQueryRepository
{
    private readonly SqlConnection _connection;

    public QueryRepository()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        _connection = new SqlConnection(configuration.GetConnectionString("IdentityConnection"));
    }
    public IDbConnection Connection => _connection;
}