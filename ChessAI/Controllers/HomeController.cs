using ChessAI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChessAI.Controllers
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
            return View();
        }

        public IActionResult Rules()
        {
            return View();
        }
        public IActionResult Play()
        {
            return View();
        }
        public IActionResult Victory()
        {
            return View();
        }
        public IActionResult Defeat()
        {
            return View();
        }
        public IActionResult Draw()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
