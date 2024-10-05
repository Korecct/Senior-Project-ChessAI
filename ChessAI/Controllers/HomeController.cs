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
        [ValidateAntiForgeryToken]
        public IActionResult MakeMove([FromBody] MoveRequest move)
        {
            var game = HttpContext.Session.GetObjectFromJson<Game>("Game");
            if (game == null)
            {
                _logger.LogWarning("Game not found in session.");
                return BadRequest("Game not found.");
            }

            // Prevent any moves if the game is over
            if (game.IsGameOver)
            {
                _logger.LogWarning("Attempted to make a move after the game is over.");
                return BadRequest("The game is already over.");
            }

            // Track game state changes
            bool wasInCheckBeforeMove = game.Board.isKingInCheck(game.IsWhiteTurn);
            bool wasInCheckAfterMove = false;
            bool isCapture = game.Board.Squares[move.ToRow][move.ToCol] != null;
            bool isPromotion = false;
            bool isCastle = false;

            var success = game.MakeMove((move.FromRow, move.FromCol), (move.ToRow, move.ToCol), _logger);
            if (!success)
            {
                _logger.LogWarning("Invalid move attempted.");
                return BadRequest("Invalid move.");
            }

            // Checks if the opponent is now in check
            wasInCheckAfterMove = game.Board.isKingInCheck(!game.IsWhiteTurn);

            // Checks for pawn promotion
            if (game.Board.Squares[move.ToRow][move.ToCol] is Pawn pawn && (move.ToRow == 0 || move.ToRow == 7))
            {
                isPromotion = true;
            }

            // Checks for castling
            if (Math.Abs(move.FromCol - move.ToCol) == 2 && game.Board.Squares[move.ToRow][move.ToCol] is King)
            {
                isCastle = true;
            }

            HttpContext.Session.SetObjectAsJson("Game", game);

            // Prepare response with game state
            var response = new
            {
                success = true,
                isGameOver = game.IsGameOver,
                gameResult = game.GameResult,
                isCheckmate = game.IsGameOver && game.GameResult.Contains("wins"),
                isCheck = wasInCheckAfterMove,
                isCapture = isCapture,
                isPromotion = isPromotion,
                isCastle = isCastle
            };

            return Json(response);
        }

        [HttpPost]
        public IActionResult GetValidMoves([FromBody] PositionModel position)
        {
            // Retrieve the game from session
            var game = HttpContext.Session.GetObjectFromJson<Game>("Game");
            if (game == null)
            {
                _logger.LogWarning("Game not found in session.");
                return BadRequest("Game not found.");
            }

            var piece = game.Board.Squares[position.Row][position.Col];
            if (piece == null || piece.IsWhite != game.IsWhiteTurn)
            {
                _logger.LogWarning("Invalid piece selected or not player's turn.");
                return BadRequest("Invalid piece selected.");
            }

            var validMoves = piece.GetValidMoves(game.Board);

            // Filter out moves that would put own king in check
            var safeMoves = new List<PositionModel>();

            foreach (var move in validMoves)
            {
                // Simulate the move
                var originalPosition = piece.Position;
                var capturedPiece = game.Board.Squares[move.Row][move.Col];

                game.Board.Squares[originalPosition.Row][originalPosition.Col] = null;
                game.Board.Squares[move.Row][move.Col] = piece;
                piece.Position = (move.Row, move.Col);

                // Check if own king is in check
                bool isInCheck = game.Board.isKingInCheck(piece.IsWhite);

                // Undo the move
                game.Board.Squares[originalPosition.Row][originalPosition.Col] = piece;
                game.Board.Squares[move.Row][move.Col] = capturedPiece;
                piece.Position = originalPosition;

                if (!isInCheck)
                {
                    safeMoves.Add(new PositionModel { Row = move.Row, Col = move.Col });
                }
            }

            // Return the valid moves as JSON with camelCase property names
            return Json(safeMoves);
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

        public class MoveRequest
        {
            public int FromRow { get; set; }
            public int FromCol { get; set; }
            public int ToRow { get; set; }
            public int ToCol { get; set; }
        }

        public class PositionModel
        {
            public int Row { get; set; }
            public int Col { get; set; }
        }
    }
}
