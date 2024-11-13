using static ChessAI.Controllers.HomeController;

namespace ChessAI.Models.AIs
{
    public class MinimaxAI : IAIPlayer
    {
        private const int MaxDepth = 3;  // Depth of the search tree for Minimax

        private static readonly int[,] PawnPositionValues =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 5, -5, -10, 0, 0, -10, -5, 5 },
            { 5, 10, 10, -20, -20, 10, 10, 5 },
            { 0, 0, 0, 20, 20, 0, 0, 0 },
            { 5, 5, 10, 25, 25, 10, 5, 5 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        private static readonly int[,] KnightPositionValues =
        {
            { -50, -40, -30, -30, -30, -30, -40, -50 },
            { -40, -20, 0, 5, 5, 0, -20, -40 },
            { -30, 5, 10, 15, 15, 10, 5, -30 },
            { -30, 0, 15, 20, 20, 15, 0, -30 },
            { -30, 5, 15, 20, 20, 15, 5, -30 },
            { -30, 0, 10, 15, 15, 10, 0, -30 },
            { -40, -20, 0, 5, 5, 0, -20, -40 },
            { -50, -40, -30, -30, -30, -30, -40, -50 }
        };

        private static readonly int[,] BishopPositionValues =
        {
            { -20, -10, -10, -10, -10, -10, -10, -20 },
            { -10, 0, 5, 10, 10, 5, 0, -10 },
            { -10, 5, 10, 15, 15, 10, 5, -10 },
            { -10, 10, 15, 20, 20, 15, 10, -10 },
            { -10, 5, 10, 15, 15, 10, 5, -10 },
            { -10, 0, 5, 10, 10, 5, 0, -10 },
            { -20, -10, -10, -10, -10, -10, -10, -20 },
            { -20, -10, -10, -10, -10, -10, -10, -20 }
        };

        private static readonly int[,] RookPositionValues =
        {
            { 0, 0, 0, 5, 5, 0, 0, 0 },
            { 0, 0, 0, 10, 10, 0, 0, 0 },
            { 0, 0, 5, 10, 10, 5, 0, 0 },
            { 0, 0, 10, 15, 15, 10, 0, 0 },
            { 0, 0, 10, 15, 15, 10, 0, 0 },
            { 0, 0, 5, 10, 10, 5, 0, 0 },
            { 0, 0, 0, 10, 10, 0, 0, 0 },
            { 0, 0, 0, 5, 5, 0, 0, 0 }
        };

        private static readonly int[,] QueenPositionValues =
        {
            { -20, -10, -10, -5, -5, -10, -10, -20 },
            { -10, 0, 5, 0, 0, 5, 0, -10 },
            { -10, 5, 10, 5, 5, 10, 5, -10 },
            { -5, 0, 5, 10, 10, 5, 0, -5 },
            { -5, 0, 5, 10, 10, 5, 0, -5 },
            { -10, 5, 10, 5, 5, 10, 5, -10 },
            { -10, 0, 5, 0, 0, 5, 0, -10 },
            { -20, -10, -10, -5, -5, -10, -10, -20 }
        };

        private static readonly int[,] KingPositionValues =
        {
            { 20, 20, 10, 0, 0, 10, 20, 20 },
            { 10, 10, 0, 0, 0, 0, 10, 10 },
            { -10, -20, -30, -30, -30, -30, -20, -10 },
            { -20, -30, -40, -40, -40, -40, -30, -20 },
            { -30, -40, -50, -50, -50, -50, -40, -30 },
            { -30, -40, -50, -50, -50, -50, -40, -30 },
            { -30, -40, -50, -50, -50, -50, -40, -30 },
            { -30, -40, -50, -50, -50, -50, -40, -30 }
        };


        public string Name => "Minimax AI";

        public (PositionModel From, PositionModel To) GetNextMove(Game game)
        {
            var board = game.Board.Clone(); // Clone the board
            var isWhiteTurn = game.IsWhiteTurn;

            // Call Minimax algorithm to get the best move
            var bestMove = GetBestMove(game, board, MaxDepth, isWhiteTurn, int.MinValue, int.MaxValue);

            return bestMove;
        }

        private (PositionModel From, PositionModel To) GetBestMove(Game game, Board board, int depth, bool isWhiteTurn, int alpha, int beta)
        {
            // Get all pieces belonging to the current player
            var aiPieces = board.Squares.SelectMany(row => row)
                                        .Where(piece => piece != null && piece.IsWhite == isWhiteTurn)
                                        .ToList();

            int bestScore = isWhiteTurn ? int.MinValue : int.MaxValue;
            (PositionModel From, PositionModel To) bestMove = (null, null);

            foreach (var piece in aiPieces)
            {
                var validMoves = piece.GetValidMoves(board).Where(move => IsMoveValid(board, piece, move)).ToList();

                foreach (var move in validMoves)
                {
                    // Clone the board for simulation
                    var boardClone = board.Clone();

                    var pieceClone = boardClone.Squares[piece.Position.Row][piece.Position.Col];
                    var targetPiece = boardClone.Squares[move.Row][move.Col];

                    // Simulate the move on the cloned board
                    boardClone.Squares[pieceClone.Position.Row][pieceClone.Position.Col] = null;
                    boardClone.Squares[move.Row][move.Col] = pieceClone.Clone(); // Clone the piece being moved
                    boardClone.Squares[move.Row][move.Col].Position = move;

                    // Handle special moves
                    if (pieceClone is King movingKing && Math.Abs(move.Col - pieceClone.Position.Col) == 2)
                    {
                        // Castling
                        int rookFromCol = move.Col > pieceClone.Position.Col ? 7 : 0;
                        int rookToCol = move.Col > pieceClone.Position.Col ? move.Col - 1 : move.Col + 1;
                        var rook = boardClone.Squares[movingKing.Position.Row][rookFromCol] as Rook;
                        if (rook != null)
                        {
                            boardClone.Squares[movingKing.Position.Row][rookFromCol] = null;
                            var rookClone = rook.Clone();
                            rookClone.Position = (movingKing.Position.Row, rookToCol);
                            boardClone.Squares[movingKing.Position.Row][rookToCol] = rookClone;
                        }
                    }

                    if (pieceClone is Pawn movingPawn)
                    {
                        // En Passant Capture
                        if (boardClone.IsEmpty(move.Row, move.Col) && Math.Abs(move.Col - pieceClone.Position.Col) == 1)
                        {
                            int capturedPawnRow = isWhiteTurn ? move.Row + 1 : move.Row - 1;
                            var capturedPawn = boardClone.Squares[capturedPawnRow][move.Col] as Pawn;
                            if (capturedPawn != null && capturedPawn.EnPassantEligible)
                            {
                                boardClone.Squares[capturedPawnRow][move.Col] = null;
                            }
                        }

                        // Promotion (assuming promotion to Queen for simplicity)
                        if ((isWhiteTurn && move.Row == 0) || (!isWhiteTurn && move.Row == 7))
                        {
                            boardClone.Squares[move.Row][move.Col] = new Queen
                            {
                                IsWhite = isWhiteTurn,
                                Position = move
                            };
                        }
                    }

                    // Recursively evaluate the position
                    int score = Minimax(game, boardClone, depth - 1, !isWhiteTurn, alpha, beta);

                    // Update bestScore and bestMove based on the evaluation
                    if (isWhiteTurn)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = (new PositionModel { Row = piece.Position.Row, Col = piece.Position.Col }, new PositionModel { Row = move.Row, Col = move.Col });
                        }
                        alpha = Math.Max(alpha, score);
                    }
                    else
                    {
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMove = (new PositionModel { Row = piece.Position.Row, Col = piece.Position.Col }, new PositionModel { Row = move.Row, Col = move.Col });
                        }
                        beta = Math.Min(beta, score);
                    }

                    // Alpha-Beta Pruning
                    if (beta <= alpha)
                    {
                        break; // Prune the search tree
                    }
                }
            }

            return bestMove;
        }

        private int Minimax(Game game, Board board, int depth, bool isWhiteTurn, int alpha, int beta)
        {
            // Base case: if we have reached the maximum depth or the game is over
            if (depth == 0 || game.IsGameOver)
            {
                return EvaluateBoard(board, isWhiteTurn, game);
            }

            // Get all pieces belonging to the current player
            var aiPieces = board.Squares.SelectMany(row => row)
                                        .Where(piece => piece != null && piece.IsWhite == isWhiteTurn)
                                        .ToList();

            int bestScore = isWhiteTurn ? int.MinValue : int.MaxValue;

            foreach (var piece in aiPieces)
            {
                var validMoves = piece.GetValidMoves(board).Where(move => IsMoveValid(board, piece, move)).ToList();

                foreach (var move in validMoves)
                {
                    // Clone the board for simulation
                    var boardClone = board.Clone();

                    var pieceClone = boardClone.Squares[piece.Position.Row][piece.Position.Col];
                    var targetPiece = boardClone.Squares[move.Row][move.Col];

                    // Simulate the move on the cloned board
                    boardClone.Squares[pieceClone.Position.Row][pieceClone.Position.Col] = null;
                    boardClone.Squares[move.Row][move.Col] = pieceClone.Clone(); // Clone the piece being moved
                    boardClone.Squares[move.Row][move.Col].Position = move;

                    // Handle special moves
                    if (pieceClone is King movingKing && Math.Abs(move.Col - pieceClone.Position.Col) == 2)
                    {
                        // Castling
                        int rookFromCol = move.Col > pieceClone.Position.Col ? 7 : 0;
                        int rookToCol = move.Col > pieceClone.Position.Col ? move.Col - 1 : move.Col + 1;
                        var rook = boardClone.Squares[movingKing.Position.Row][rookFromCol] as Rook;
                        if (rook != null)
                        {
                            boardClone.Squares[movingKing.Position.Row][rookFromCol] = null;
                            var rookClone = rook.Clone();
                            rookClone.Position = (movingKing.Position.Row, rookToCol);
                            boardClone.Squares[movingKing.Position.Row][rookToCol] = rookClone;
                        }
                    }

                    if (pieceClone is Pawn movingPawn)
                    {
                        // En Passant Capture
                        if (boardClone.IsEmpty(move.Row, move.Col) && Math.Abs(move.Col - pieceClone.Position.Col) == 1)
                        {
                            int capturedPawnRow = isWhiteTurn ? move.Row + 1 : move.Row - 1;
                            var capturedPawn = boardClone.Squares[capturedPawnRow][move.Col] as Pawn;
                            if (capturedPawn != null && capturedPawn.EnPassantEligible)
                            {
                                boardClone.Squares[capturedPawnRow][move.Col] = null;
                            }
                        }

                        // Promotion (assuming promotion to Queen for simplicity)
                        if ((isWhiteTurn && move.Row == 0) || (!isWhiteTurn && move.Row == 7))
                        {
                            boardClone.Squares[move.Row][move.Col] = new Queen
                            {
                                IsWhite = isWhiteTurn,
                                Position = move
                            };
                        }
                    }

                    // Recursively evaluate the position
                    int score = Minimax(game, boardClone, depth - 1, !isWhiteTurn, alpha, beta);

                    // Update bestScore based on the evaluation
                    if (isWhiteTurn)
                    {
                        bestScore = Math.Max(bestScore, score);
                        alpha = Math.Max(alpha, bestScore);
                    }
                    else
                    {
                        bestScore = Math.Min(bestScore, score);
                        beta = Math.Min(beta, bestScore);
                    }

                    // Alpha-Beta Pruning
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }

            return bestScore;
        }

        // Evaluation function
        private int EvaluateBoard(Board board, bool isWhiteTurn, Game game)
        {
            if (game.IsGameOver)
            {
                if (game.GameResult.Contains("Checkmate"))
                {
                    return isWhiteTurn ? int.MaxValue : int.MinValue;  // Checkmate: a win for the current player
                }
                else if (game.GameResult.Contains("Draw"))
                {
                    return 0;  // Draw
                }
            }

            int evaluation = 0;
            var pieceValues = new Dictionary<Type, int>
            {
                { typeof(Pawn), 100 },
                { typeof(Knight), 320 },
                { typeof(Bishop), 330 },
                { typeof(Rook), 500 },
                { typeof(Queen), 900 },
                { typeof(King), 20000 }
            };

            var piecePositionValues = new Dictionary<Type, int[,]>
            {
                { typeof(Pawn), PawnPositionValues },
                { typeof(Knight), KnightPositionValues },
                { typeof(Bishop), BishopPositionValues },
                { typeof(Rook), RookPositionValues },
                { typeof(Queen), QueenPositionValues },
                { typeof(King), KingPositionValues }
            };

            foreach (var row in board.Squares)
            {
                foreach (var piece in row)
                {
                    if (piece != null)
                    {
                        int value = pieceValues[piece.GetType()];
                        int positionValue = piecePositionValues[piece.GetType()][piece.Position.Row, piece.Position.Col];

                        int totalValue = value + positionValue;

                        if (piece.IsWhite == isWhiteTurn)
                        {
                            evaluation += totalValue;
                        }
                        else
                        {
                            evaluation -= totalValue;
                        }
                    }
                }
            }

            return evaluation;
        }

        // Check if the move is valid
        private bool IsMoveValid(Board board, Piece piece, (int Row, int Col) move)
        {
            int toRow = move.Row;
            int toCol = move.Col;

            // Check if the move is within bounds
            if (toRow < 0 || toRow >= 8 || toCol < 0 || toCol >= 8)
            {
                return false;
            }

            // Clone the board to simulate the move
            var boardClone = board.Clone();
            var pieceClone = boardClone.Squares[piece.Position.Row][piece.Position.Col];
            var capturedPiece = boardClone.Squares[toRow][toCol];

            // Simulate the move on the cloned board
            boardClone.Squares[pieceClone.Position.Row][pieceClone.Position.Col] = null;
            boardClone.Squares[toRow][toCol] = pieceClone.Clone();
            boardClone.Squares[toRow][toCol].Position = (toRow, toCol);

            // Handle En Passant capture in simulation
            Piece enPassantCapturedPawn = null;
            if (pieceClone is Pawn pawn && board.IsEmpty(toRow, toCol) && toCol != piece.Position.Col)
            {
                // En Passant capture
                int capturedPawnRow = piece.IsWhite ? toRow + 1 : toRow - 1;
                enPassantCapturedPawn = boardClone.Squares[capturedPawnRow][toCol];
                boardClone.Squares[capturedPawnRow][toCol] = null;
            }

            // Check if the king is in check after the move
            bool isInCheck = boardClone.IsKingInCheck(piece.IsWhite);

            // Restore captured pawn if en passant
            if (enPassantCapturedPawn != null)
            {
                int capturedPawnRow = piece.IsWhite ? toRow + 1 : toRow - 1;
                boardClone.Squares[capturedPawnRow][toCol] = enPassantCapturedPawn;
            }

            return !isInCheck;
        }
    }
}
