using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SSO.Domain.Entities.Roles;
using SSO.Domain.Entities.Users;

namespace SSO.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "BFF")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IRoleRepository _roleRepository;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger
            , UserManager<User> userManager
            , RoleManager<Role> roleManager
            , IRoleRepository roleRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var _claims = User.Claims.Select(claim => new { claim.Type, claim.Value }).ToArray();
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var item in roles)
            {
                var role = await _roleRepository.GetAsync(_ => _.Name == item);
                var claims = await _roleManager.GetClaimsAsync(role);
                if (true)
                {
                    
                }
            }
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}