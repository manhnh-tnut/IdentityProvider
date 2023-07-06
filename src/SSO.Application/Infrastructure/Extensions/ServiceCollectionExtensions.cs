using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using SSO.Application.Infrastructure.Filters;
using SSO.Domain.Entities.Roles;
using SSO.Domain.Entities.Users;
using SSO.Infrastructure.Contexts;

namespace SSO.Application.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, Role>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddDefaultUI()
            .AddEntityFrameworkStores<EFContext>()
            .AddDefaultTokenProviders();

            return services;
        }

        public static IServiceCollection AddCustomAspNetIdentity(this IServiceCollection services)
        {

            #region AspNetIdentity
            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));
            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
                options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;

                options.SignIn.RequireConfirmedAccount = false;

                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ";
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });
            #endregion

            return services;
        }

        public static IServiceCollection AddCustomOpenId(this IServiceCollection services)
        {

            #region OpenId
            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(builder =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    builder.UseEntityFrameworkCore()
                        .UseDbContext<EFContext>();
                })
                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the token endpoint.
                    options.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetTokenEndpointUris("/connect/token")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetVerificationEndpointUris("/connect/verify");

                    options.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);

                    // Enable the client credentials flow.
                    options.AllowClientCredentialsFlow()
                        .AllowAuthorizationCodeFlow()
                        .AllowRefreshTokenFlow()
                        .RequireProofKeyForCodeExchange();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableStatusCodePagesIntegration();
                })
                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();
                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                });
            // Register the worker responsible of seeding the database with the sample clients.
            // Note: in a real world application, this step should be part of a setup script.
            services.AddHostedService<Worker>();
            services.AddHostedService<Seed>();
            #endregion

            return services;
        }

        public static IServiceCollection AddCustomController(this IServiceCollection services)
        {
            // Add framework services.
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(ErrorHandlingFilter));
            })
                // Added for functional tests
                //.AddApplicationPart(typeof(HomeController).Assembly)
                .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

            //services.AddCors(options =>
            //{
            //    options.AddPolicy("CorsPolicy",
            //        builder => builder
            //        .SetIsOriginAllowed((host) => true)
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials());
            //});

            return services;
        }

    }
}
