using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ChessAI.Models
{
    [Serializable]
    public class Game
    {
        public Board Board { get; set; } = new Board();
        public bool IsWhiteTurn { get; set; } = true;

        public bool MakeMove((int Row, int Col) from, (int Row, int Col) to, ILogger logger)
        {
            var piece = Board.Squares[from.Row, from.Col];
            if (piece == null)
            {
                logger.LogInformation($"No piece at position ({from.Row}, {from.Col}).");
                return false;
            }

            if (piece.IsWhite != IsWhiteTurn)
            {
                logger.LogInformation($"It's not {(piece.IsWhite ? "white" : "black")}'s turn.");
                return false;
            }

            var validMoves = piece.GetValidMoves(Board);
            logger.LogInformation($"Valid moves for piece at ({from.Row}, {from.Col}): {string.Join(", ", validMoves.Select(m => $"({m.Row}, {m.Col})"))}");

            if (validMoves.Any(m => m.Row == to.Row && m.Col == to.Col))
            {
                var originalPosition = piece.Position;
                Board.Squares[to.Row, to.Col] = piece;
                Board.Squares[from.Row, from.Col] = null;
                piece.Position = to;
                if (Board.Squares[from.Row, to.Col] is Pawn pawn) //En-Passant Capture logic
                {
                    if (pawn.IsWhite != true)
                    {
                        Board.Squares[3, to.Col] = null;
                    }
                    if (pawn.IsWhite == true)
                    {
                        Board.Squares[4, to.Col] = null;
                    }
                }

                if (Board.isKingInCheck(IsWhiteTurn))
                {
                    Board.Squares[from.Row, from.Col] = piece;
                    Board.Squares[to.Row, to.Col] = null;
                    piece.Position = originalPosition;
                    logger.LogInformation($"Move to ({to.Row}, {to.Col}) puts the king in check. Move aborted.");
                    return false;
                }

                if (Board.isKingInCheck(!IsWhiteTurn))
                {
                    logger.LogInformation($"{(!IsWhiteTurn ? "White" : "Black")} king is now in check after the move.");

                    // Check for escape moves
                    if (Board.AreAnyEscapeMovesAvailable(!IsWhiteTurn))
                    {
                        logger.LogInformation($"{(IsWhiteTurn ? "Black" : "White")} king is in check but has escape moves available.");
                    }
                    else
                    {
                        logger.LogInformation($"{(IsWhiteTurn ? "Black" : "White")} king is in check and has no escape moves available. Checkmate may be imminent.");
                    }
                }

                IsWhiteTurn = !IsWhiteTurn;
                return true;
            }
            else
            {
                logger.LogInformation($"Move to ({to.Row}, {to.Col}) is not valid for piece at ({from.Row}, {from.Col}).");
                return false;
            }
        }
    }
}

