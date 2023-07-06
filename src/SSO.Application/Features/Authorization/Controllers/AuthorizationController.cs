using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SSO.Application.Features.Authorization.Responses;
using SSO.Application.Infrastructure.Attributes;
using System.Collections.Immutable;
using System.Security.Claims;

namespace SSO.Application.Features.Authorization.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly SignInManager<SSO.Domain.Entities.Users.User> _signInManager;
        private readonly UserManager<SSO.Domain.Entities.Users.User> _userManager;

        public AuthorizationController(
            ILogger<AuthorizationController> logger
            , IOpenIddictApplicationManager applicationManager
            , IOpenIddictAuthorizationManager authorizationManager
            , IOpenIddictScopeManager scopeManager
            , SignInManager<SSO.Domain.Entities.Users.User> signInManager
            , UserManager<SSO.Domain.Entities.Users.User> userManager)
        {
            _logger = logger ?? throw new NullReferenceException(nameof(logger));
            _applicationManager = applicationManager ?? throw new NullReferenceException(nameof(applicationManager));
            _authorizationManager = authorizationManager ?? throw new NullReferenceException(nameof(authorizationManager));
            _scopeManager = scopeManager ?? throw new NullReferenceException(nameof(scopeManager));
            _signInManager = signInManager ?? throw new NullReferenceException(nameof(signInManager));
            _userManager = userManager ?? throw new NullReferenceException(nameof(userManager));
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID connect request cannot be retrieved.");
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (result == null || !result.Succeeded || request.HasPrompt(OpenIddictConstants.Prompts.Login)
                || (request.MaxAge != null && result.Properties?.IssuedUtc != null
                    && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {
                if (request.HasPrompt(OpenIddictConstants.Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        , properties: new AuthenticationProperties(items: new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in.",
                        })
                    );
                }

                var prompt = string.Join(" ", request.GetPrompts().Remove(OpenIddictConstants.Prompts.Login));
                var parameters = Request.HasFormContentType ?
                    Request.Form.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList() :
                    Request.Query.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList();

                parameters.Add(KeyValuePair.Create(OpenIddictConstants.Parameters.Prompt, new StringValues(prompt)));
                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme
                    , properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                    }
                );
            }

            var user = await _userManager.GetUserAsync(result.Principal) ?? throw new InvalidOperationException("The user details cannot be retrieved.");
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user)
                , client: await _applicationManager.GetIdAsync(application) ?? string.Empty
                , status: OpenIddictConstants.Statuses.Valid
                , type: OpenIddictConstants.AuthorizationTypes.Permanent
                , scopes: request.GetScopes()
            ).ToListAsync();

            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                case OpenIddictConstants.ConsentTypes.External when !authorizations.Any():
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        , properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allow to access this client application."
                        })
                    );
                case OpenIddictConstants.ConsentTypes.Implicit:
                case OpenIddictConstants.ConsentTypes.External when authorizations.Any():
                case OpenIddictConstants.ConsentTypes.Explicit when authorizations.Any() && request.HasPrompt(OpenIddictConstants.Prompts.Consent):
                    // Create a new ClaimsIdentity containing the claims that
                    // will be used to create an id_token, a token or a code.
                    var identity = new ClaimsIdentity(
                        authenticationType: TokenValidationParameters.DefaultAuthenticationType
                        , nameType: OpenIddictConstants.Claims.Name
                        , roleType: OpenIddictConstants.Claims.Role
                    );


                    identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                        .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                        .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
                        .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

                    identity.SetDestinations(GetDestinations);

                    identity.SetScopes(request.GetScopes());
                    identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

                    var authorization = authorizations.LastOrDefault();
                    authorization ??= await _authorizationManager.CreateAsync(
                        identity: identity
                        , subject: await _userManager.GetUserIdAsync(user)
                        , client: await _applicationManager.GetIdAsync(application) ?? string.Empty
                        , type: OpenIddictConstants.AuthorizationTypes.Permanent
                        , scopes: identity.GetScopes()
                    );

                    identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                case OpenIddictConstants.ConsentTypes.Explicit when request.HasPrompt(OpenIddictConstants.Prompts.None):
                case OpenIddictConstants.ConsentTypes.Systematic when request.HasPrompt(OpenIddictConstants.Prompts.None):
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        , properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required."
                        })
                    );
                default:

                    return View("~/Features/Authorization/Pages/Authorize.cshtml", new AuthorizeResponseModel(
                        applicationName: await _applicationManager.GetLocalizedDisplayNameAsync(application) ?? string.Empty
                        , scopes: request.Scope ?? string.Empty
                    ));
            }
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID connect request cannot be retrieved.");
            var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("The user details cannot be retrieved.");
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");
            var authorizations = await _authorizationManager.FindAsync(
               subject: await _userManager.GetUserIdAsync(user)
               , client: await _applicationManager.GetIdAsync(application) ?? string.Empty
               , status: OpenIddictConstants.Statuses.Valid
               , type: OpenIddictConstants.AuthorizationTypes.Permanent
               , scopes: request.GetScopes()
           ).ToListAsync();

            if (!authorizations.Any() && await _applicationManager.HasConsentTypeAsync(application, OpenIddictConstants.ConsentTypes.External))
            {
                return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        , properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allow to access this client application."
                        })
                    );
            }

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType
                , nameType: OpenIddictConstants.Claims.Name
                , roleType: OpenIddictConstants.Claims.Role
            );

            identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

            identity.SetDestinations(GetDestinations);

            identity.SetScopes(request.GetScopes());
            identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            var authorization = authorizations.LastOrDefault();
            authorization ??= await _authorizationManager.CreateAsync(
                identity: identity
                , subject: await _userManager.GetUserIdAsync(user)
                , client: await _applicationManager.GetIdAsync(application) ?? string.Empty
                , type: OpenIddictConstants.AuthorizationTypes.Permanent
                , scopes: identity.GetScopes()
            );

            identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public IActionResult Deny()
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/logout")]
        public IActionResult Logout()
        {
            return View("~/Features/Authorization/Pages/Logout.cshtml");
        }

        [ActionName(nameof(Logout)), HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                , properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                }
            );
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID connect request cannot be retrieved.");

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var user = await _userManager.FindByIdAsync(result.Principal?.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);
                if (user is null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        , properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                        })
                    );
                }

                if (!await _signInManager.CanSignInAsync(user))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        , properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sing in."
                        })
                    );
                }

                // Create a new ClaimsIdentity containing the claims that
                // will be used to create an id_token, a token or a code.
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType
                    , nameType: OpenIddictConstants.Claims.Name
                    , roleType: OpenIddictConstants.Claims.Role
                );

                // Use the client_id as the subject identifier.
                identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not sopported.");
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            if (claim.Subject is not null)
            {
                switch (claim.Type)
                {
                    case OpenIddictConstants.Claims.Name:
                        yield return OpenIddictConstants.Destinations.AccessToken;
                        if (claim.Subject.HasClaim(OpenIddictConstants.Claims.Name))
                            yield return OpenIddictConstants.Destinations.IdentityToken;
                        yield break;

                    case OpenIddictConstants.Claims.Email:
                        yield return OpenIddictConstants.Destinations.AccessToken;
                        if (claim.Subject.HasClaim(OpenIddictConstants.Claims.Email))
                            yield return OpenIddictConstants.Destinations.IdentityToken;
                        yield break;

                    case OpenIddictConstants.Claims.Role:
                        yield return OpenIddictConstants.Destinations.AccessToken;
                        if (claim.Subject.HasClaim(OpenIddictConstants.Claims.Role))
                            yield return OpenIddictConstants.Destinations.IdentityToken;
                        yield break;

                    case "AspNet.Identity.SecurityStamp":
                        yield break;

                    default:
                        yield return OpenIddictConstants.Destinations.AccessToken;
                        yield break;
                }
            }
        }
    }
}
