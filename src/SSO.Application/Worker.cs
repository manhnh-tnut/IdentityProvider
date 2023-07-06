using OpenIddict.Abstractions;
using SSO.Infrastructure.Contexts;
using System.Globalization;

namespace SSO.Application
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<EFContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            #region BFF
            // Resource
            if (await applicationManager.FindByClientIdAsync("BFF", cancellationToken) is null)
            {
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "BFF",
                    ClientSecret = "BFF-Secret",
                    DisplayName = "BFF client application",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                    }
                };

                await applicationManager.CreateAsync(descriptor, cancellationToken);
            }

            // Scope
            if (await scopeManager.FindByNameAsync("bffapi", cancellationToken) is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "bffapi",
                    Resources = { "BFF" },
                    DisplayName = "BFF Api Access",
                    DisplayNames = { [CultureInfo.GetCultureInfo("en-US")] = "BFF API" },
                }, cancellationToken);
            }
            #endregion

            #region Postman
            // Client
            var postman = await applicationManager.FindByClientIdAsync("Postman", cancellationToken);
            if (postman is not null)
            {
                await applicationManager.DeleteAsync(postman, cancellationToken);
            }
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "Postman",
                ClientSecret = "Postman-Secret",
                DisplayName = "Postman client application",
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope +"bffapi",
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            }, cancellationToken);
            #endregion

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
