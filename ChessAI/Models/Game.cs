namespace ChessAI.Models
{
    [Serializable]
    public class Game
    {
        public Board Board { get; set; } = new Board();
        public bool IsWhiteTurn { get; set; } = true;
        public bool IsGameOver { get; set; } = false;
        public string GameResult { get; set; } = "";

        public bool MakeMove((int Row, int Col) from, (int Row, int Col) to, ILogger logger)
        {
            var piece = Board.Squares[from.Row][from.Col];
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

            // Filter out moves that would put own king in check
            validMoves = validMoves.Where(move =>
            {
                // Store original position
                var originalPosition = piece.Position;
                var capturedPiece = Board.Squares[move.Row][move.Col];

                // Simulate the move
                Board.Squares[originalPosition.Row][originalPosition.Col] = null;
                Board.Squares[move.Row][move.Col] = piece;
                piece.Position = move;

                // Handle En Passant capture in simulation
                Piece enPassantCapturedPawn = null;
                if (piece is Pawn && Board.Squares[move.Row][move.Col] == null && move.Col != originalPosition.Col)
                {
                    // capture
                    int capturedPawnRow = originalPosition.Row;
                    enPassantCapturedPawn = Board.Squares[capturedPawnRow][move.Col];
                    Board.Squares[capturedPawnRow][move.Col] = null;
                }

                // Check if own king is in check
                bool isInCheck = Board.IsKingInCheck(piece.IsWhite);

                // Undo the move
                Board.Squares[originalPosition.Row][originalPosition.Col] = piece;
                Board.Squares[move.Row][move.Col] = capturedPiece;
                piece.Position = originalPosition;

                // Restore captured pawn if en passant
                if (enPassantCapturedPawn != null)
                {
                    int capturedPawnRow = originalPosition.Row;
                    Board.Squares[capturedPawnRow][move.Col] = enPassantCapturedPawn;
                }

                return !isInCheck;
            }).ToList();

            logger.LogInformation($"Valid moves for piece at ({from.Row}, {from.Col}): {string.Join(", ", validMoves.Select(m => $"({m.Row}, {m.Col})"))}");

            if (validMoves.Any(m => m.Row == to.Row && m.Col == to.Col))
            {
                var capturedPiece = Board.Squares[to.Row][to.Col];
                var originalPosition = piece.Position;

                // Castling
                if (piece is King king && Math.Abs(to.Col - from.Col) == 2)
                {
                    // Castling move
                    int rookFromCol = to.Col > from.Col ? 7 : 0;
                    int rookToCol = to.Col > from.Col ? to.Col - 1 : to.Col + 1;
                    var rook = Board.Squares[from.Row][rookFromCol] as Rook;

                    // Move the rook
                    Board.Squares[from.Row][rookFromCol] = null;
                    Board.Squares[from.Row][rookToCol] = rook;
                    rook.Position = (from.Row, rookToCol);
                    rook.HasMoved = true;
                }

                // En passant
                if (piece is Pawn pawn && Board.Squares[to.Row][to.Col] == null && to.Col != from.Col)
                {
                    // En passant capture
                    int capturedPawnRow = from.Row;
                    capturedPiece = Board.Squares[capturedPawnRow][to.Col];
                    Board.Squares[capturedPawnRow][to.Col] = null;
                }

                // Move piece
                Board.Squares[to.Row][to.Col] = piece;
                Board.Squares[from.Row][from.Col] = null;
                piece.Position = to;

                // Update piece's HasMoved property if applicable
                if (piece is Pawn movingPawn)
                {
                    // En Passant handling
                    // Reset EnPassantEligible for all pawns
                    foreach (var row in Board.Squares)
                    {
                        foreach (var p in row)
                        {
                            if (p is Pawn pwn)
                            {
                                pwn.EnPassantEligible = false;
                            }
                        }
                    }

                    if (Math.Abs(to.Row - from.Row) == 2)
                    {
                        movingPawn.EnPassantEligible = true;
                    }

                    movingPawn.HasMoved = true;
                }

                if (piece is Rook movingRook)
                {
                    movingRook.HasMoved = true;
                }

                if (piece is King movingKing)
                {
                    movingKing.HasMoved = true;
                }

                // Check for pawn promotion
                if (piece is Pawn && (to.Row == 0 || to.Row == 7))
                {
                    PromotePawn(to, piece.IsWhite, logger);
                }

                // Check for checkmate or stalemate
                if (Board.IsKingInCheck(!IsWhiteTurn))
                {
                    if (!Board.AreAnyMovesAvailable(!IsWhiteTurn))
                    {
                        // Checkmate
                        IsGameOver = true;
                        GameResult = IsWhiteTurn ? "Checkmate! White Wins!" : "Checkmate! Black Wins!";
                        logger.LogInformation($"Checkmate! {(IsWhiteTurn ? "White" : "Black")} wins.");
                        // Handle victory condition
                    }
                    else
                    {
                        logger.LogInformation($"{(!IsWhiteTurn ? "White" : "Black")} king is in check.");
                    }
                }
                else
                {
                    if (!Board.AreAnyMovesAvailable(!IsWhiteTurn))
                    {
                        // Stalemate
                        IsGameOver = true;
                        GameResult = "Stalemate. The game is a draw.";
                        logger.LogInformation("Stalemate. The game is a draw.");
                        // Handle draw condition
                    }
                    else if (Board.IsInsufficientMaterial())
                    {
                        // Draw due to insufficient material
                        IsGameOver = true;
                        GameResult = "Draw due to insufficient material.";
                        logger.LogInformation("Draw due to insufficient material.");
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
            Board.Squares[position.Row][position.Col] = newPiece;

            logger.LogInformation($"Pawn promoted to {newPiece.GetType().Name} at ({position.Row}, {position.Col}).");
        }
    }
}