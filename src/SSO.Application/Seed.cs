using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using SSO.Infrastructure.Contexts;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.Application
{
    public class Seed : IHostedService
    {
        private readonly ILogger<Seed> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Seed(
            ILogger<Seed> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            var context = scope.ServiceProvider.GetRequiredService<EFContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SSO.Domain.Entities.Users.User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<SSO.Domain.Entities.Roles.Role>>();

            #region role
            var role = new SSO.Domain.Entities.Roles.Role()
            {
                Name = "OWNER"
            };
            if (!await roleManager.RoleExistsAsync(role.Name))
            {
                await roleManager.CreateAsync(role);
            }
            else
            {
                role = await roleManager.FindByNameAsync(role.Name);
            }
            #endregion

            #region role claims
            var roleClaims = await roleManager.GetClaimsAsync(role);
            foreach (var item in new List<string>() { "create", "view", "edit", "delete" })
            {
                if (!roleClaims.Any(_ => _.Type == "Permission" && _.Value == item))
                {
                    await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", item));
                }
            }
            #endregion

            #region admin
            var user = new SSO.Domain.Entities.Users.User()
            {
                Email = "admin@example.com",
                UserName = "admin@example.com",
                EmailConfirmed = true,
                FullName = "Admin"
            };
            if (await userManager.FindByEmailAsync(user.Email) is null)
            {
                var createUserResult = await userManager.CreateAsync(user, "Abc!3579");
                if (!createUserResult.Succeeded)
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        _logger.LogError(message: error.Description);
                    }
                }
            }
            else
            {
                user = await userManager.FindByEmailAsync(user.Email);
            }

            #endregion

            #region admin claims
            var userClaims = await userManager.GetClaimsAsync(user);
            foreach (var item in new List<string>() { "bffapi" })
            {
                if (!userClaims.Any(_ => _.Type == "Permission" && _.Value == item))
                {
                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Permission", item));
                }
            }
            #endregion

            #region admin role
            if (!await userManager.IsInRoleAsync(user, role.Name))
            {
                var addRoleForUserResult = await userManager.AddToRoleAsync(user, role.Name);
                if (!addRoleForUserResult.Succeeded)
                {
                    foreach (var error in addRoleForUserResult.Errors)
                    {
                        _logger.LogError(error.Description);
                    }
                }
            }
            #endregion

        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
