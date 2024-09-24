using ChessAI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

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
        [HttpGet]
        public IActionResult Play()
        {
            // Test session availability
            HttpContext.Session.SetString("Test", "Session is working");
            var testValue = HttpContext.Session.GetString("Test");
            if (testValue == null)
            {
                return Content("Session is not working");
            }

            // Retrieve the game from the session
            var game = HttpContext.Session.GetObjectFromJson<Game>("Game");
            if (game == null)
            {
                game = new Game();
                HttpContext.Session.SetObjectAsJson("Game", game);
            }

            return View(game);
        }

        [HttpPost]
        public IActionResult MakeMove([FromBody] MoveRequest move)
        {
            var game = HttpContext.Session.GetObjectFromJson<Game>("Game");
            if (game == null)
            {
                return BadRequest("Game not found.");
            }

            var success = game.MakeMove((move.FromRow, move.FromCol), (move.ToRow, move.ToCol));
            if (!success)
            {
                return BadRequest("Invalid move.");
            }

            HttpContext.Session.SetObjectAsJson("Game", game);
            return Ok();
        }

    

    public class MoveRequest
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
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
