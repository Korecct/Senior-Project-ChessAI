using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;

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

                // Handle En-Passant capture logic
                if (Board.Squares[from.Row, to.Col] is Pawn pawn)
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

                // Check for pawn promotion
                if (piece is Pawn && (to.Row == 0 || to.Row == 7))
                {
                    PromotePawn(to, piece.IsWhite, logger);
                }

                // Check if the king is in check
                if (Board.isKingInCheck(IsWhiteTurn))
                {
                    // Undo the move
                    Board.Squares[from.Row, from.Col] = piece;
                    Board.Squares[to.Row, to.Col] = null;
                    piece.Position = originalPosition;
                    logger.LogInformation($"Move to ({to.Row}, {to.Col}) puts the king in check. Move aborted.");
                    return false;
                }

                // Check if the opposite king is in check
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

        private void PromotePawn((int Row, int Col) position, bool isWhite, ILogger logger)
        {
            // Default promotion to a queen
            Piece newPiece = new Queen { IsWhite = isWhite, Position = position };

            // Replace the pawn with the new piece
            Board.Squares[position.Row, position.Col] = newPiece;

            logger.LogInformation($"Pawn promoted to {newPiece.GetType().Name} at ({position.Row}, {position.Col}).");
        }
    }
}

