using Microsoft.AspNetCore.Mvc;

namespace SSO.Application.Features.Home.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Features/Home/Pages/Index.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("~/Features/Home/Pages/Privacy.cshtml");
        }
    }
}