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
                Piece enPassantCaptured = null;
                if (piece is Pawn movingPawn && capturedPiece == null && from.Col != to.Col)
                {
                    int capturedPawnRow = IsWhiteTurn ? move.Row + 1 : move.Row - 1;
                    enPassantCaptured = Board.Squares[capturedPawnRow][move.Col];
                    Board.Squares[capturedPawnRow][move.Col] = null;
                }

                // Check if own king is in check
                bool isInCheck = Board.isKingInCheck(piece.IsWhite);

                // Undo the move
                Board.Squares[originalPosition.Row][originalPosition.Col] = piece;
                Board.Squares[move.Row][move.Col] = capturedPiece;
                piece.Position = originalPosition;

                return !isInCheck;
            }).ToList();

            logger.LogInformation($"Valid moves for piece at ({from.Row}, {from.Col}): {string.Join(", ", validMoves.Select(m => $"({m.Row}, {m.Col})"))}");

            if (validMoves.Any(m => m.Row == to.Row && m.Col == to.Col))
            {
                // Reset En Passant eligibility for all pawns before making the move
                ResetEnPassantEligibility();

                var originalPosition = piece.Position;
                var capturedPiece = Board.Squares[to.Row][to.Col];

                // Move piece
                Board.Squares[to.Row][to.Col] = piece;
                Board.Squares[from.Row][from.Col] = null;
                piece.Position = to;

                // Update piece's HasMoved property if applicable
                if (piece is Pawn pawn)
                {
                    pawn.HasMoved = true;

                    // Handle En Passant eligibility
                    pawn.EnPassantEligible = Math.Abs(from.Row - to.Row) == 2;
                }
                else if (piece is Rook rook)
                {
                    rook.HasMoved = true;
                }
                else if (piece is King king)
                {
                    king.HasMoved = true;

                    // Handle Castling
                    if (Math.Abs(from.Col - to.Col) == 2)
                    {
                        int rookFromCol = to.Col == 6 ? 7 : 0;
                        int rookToCol = to.Col == 6 ? 5 : 3;
                        var castlingRook = Board.Squares[from.Row][rookFromCol] as Rook;

                        if (castlingRook != null)
                        {
                            Board.Squares[from.Row][rookToCol] = castlingRook;
                            Board.Squares[from.Row][rookFromCol] = null;
                            castlingRook.Position = (from.Row, rookToCol);
                            castlingRook.HasMoved = true;
                        }
                    }
                }

                // Handle En Passant capture
                if (piece is Pawn movingPawn && capturedPiece == null && from.Col != to.Col)
                {
                    int capturedPawnRow = IsWhiteTurn ? to.Row + 1 : to.Row - 1;
                    var enPassantCapturedPawn = Board.Squares[capturedPawnRow][to.Col] as Pawn;
                    if (enPassantCapturedPawn != null && enPassantCapturedPawn.EnPassantEligible)
                    {
                        Board.Squares[capturedPawnRow][to.Col] = null;
                        logger.LogInformation($"En Passant capture performed on ({capturedPawnRow}, {to.Col}).");
                    }
                }

                // Check for pawn promotion
                if (piece is Pawn && (to.Row == 0 || to.Row == 7))
                {
                    PromotePawn(to, piece.IsWhite, logger);
                }

                // Check for checkmate or stalemate
                if (Board.isKingInCheck(!IsWhiteTurn))
                {
                    if (!Board.AreAnyMovesAvailable(!IsWhiteTurn))
                    {
                        // Checkmate
                        IsGameOver = true;
                        GameResult = IsWhiteTurn ? "White wins by checkmate." : "Black wins by checkmate.";
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

        private void ResetEnPassantEligibility()
        {
            foreach (var row in Board.Squares)
            {
                foreach (var p in row)
                {
                    if (p is Pawn pawn)
                    {
                        pawn.EnPassantEligible = false;
                    }
                }
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