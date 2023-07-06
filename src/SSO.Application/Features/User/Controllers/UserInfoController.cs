using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace SSO.Application.Features.User.Controllers
{
    public class UserInfoController : Controller
    {
        private readonly ILogger<UserInfoController> _logger;
        private readonly UserManager<SSO.Domain.Entities.Users.User> _userManager;

        public UserInfoController(
            ILogger<UserInfoController> logger
            , UserManager<SSO.Domain.Entities.Users.User> userManager)
        {
            _logger = logger ?? throw new NullReferenceException(nameof(logger));
            _userManager = userManager ?? throw new NullReferenceException(nameof(userManager));
        }

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo"), Produces("application/json")]
        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.FindByIdAsync(User.GetClaim(OpenIddictConstants.Claims.Subject) ?? string.Empty);
            if (user is null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                    , properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
                    })
                );
            }
            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user)
            };
            if (User.HasScope(OpenIddictConstants.Scopes.Email))
            {
                claims[OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
                claims[OpenIddictConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }
            if (User.HasScope(OpenIddictConstants.Scopes.Phone))
            {
                claims[OpenIddictConstants.Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
                claims[OpenIddictConstants.Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
            }
            if(User.HasScope(OpenIddictConstants.Claims.Role))
            {
                claims[OpenIddictConstants.Claims.Role] = await _userManager.GetRolesAsync(user);
            }

            return Ok(claims);
        }
    }
}
