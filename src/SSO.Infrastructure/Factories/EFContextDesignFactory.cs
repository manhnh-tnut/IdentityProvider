using Azure;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SSO.Infrastructure.Contexts;
using System.Reflection;

namespace PRO.Api.Infrastructure.Factories;

public class EFContextDesignFactory : IDesignTimeDbContextFactory<EFContext>
{
    public EFContext CreateDbContext(string[] args)
    {
        // Get environment
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        // Build config
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SSO.Application"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string
        var connectionString = config.GetConnectionString("IdentityConnection");
        var optionsBuilder = new DbContextOptionsBuilder<EFContext>();
        optionsBuilder.UseSqlServer(
            connectionString: connectionString
            , sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(EFContext).GetTypeInfo().Assembly.GetName().Name);
                //sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            }
        );

        return new EFContext(optionsBuilder.Options, new NoMediator());
    }

    class NoMediator : IMediator
    {
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return default;
        }

        public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            return default;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TResponse));
        }

        public Task<object> Send(object request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(object));
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            return Task.FromResult(default(TRequest));
        }
    }
}
