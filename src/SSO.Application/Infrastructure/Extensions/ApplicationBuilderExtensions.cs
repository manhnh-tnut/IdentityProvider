using Autofac.Extensions.DependencyInjection;
using Autofac;
using Serilog;
using SSO.Application.Infrastructure.AutofacModules;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SSO.Infrastructure.Contexts;
using Autofac.Core;

namespace SSO.Application.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions();

            #region Serilog
            builder.Configuration.AddJsonFile("serilog.json", optional: true, reloadOnChange: true);
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console()
                .ReadFrom.Configuration(ctx.Configuration)
            );
            #endregion

            #region AutoFac
            // Autofac provider to the generic hosting mechanism.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Services.AddAutofac();
            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
            #endregion

            #region MediatR
            builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            builder.Host.ConfigureContainer<ContainerBuilder>(c =>
            {
                c.RegisterModule(new MediatorModule());
                c.RegisterModule(new ApplicationModule());
            });
            #endregion

            #region Cors
            builder.Services.AddCors(options =>
                        {
                            options.AddPolicy("SSO.CorsPolicy",
                                builder => builder
                                .SetIsOriginAllowed((host) => true)
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials());
                        });
            #endregion

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
            return builder;
        }

        public static IServiceCollection ConfigureDbContext(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("IdentityConnection") ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found.");
            builder.Services.AddDbContext<EFContext>(options =>
            {
                options.UseSqlServer(
                    connectionString: connectionString
                    , sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(EFContext).GetTypeInfo().Assembly.GetName().Name);
                        //sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                    }
                );
                options.UseOpenIddict();
            },
            //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
            ServiceLifetime.Scoped);
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            return builder.Services;
        }

    }
}