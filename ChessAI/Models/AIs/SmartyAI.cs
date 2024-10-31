using static ChessAI.Controllers.HomeController;
namespace ChessAI.Models.AIs
{
    public class SmartyAI : IAIPlayer
    {
        public string Name => "Smarty AI";
        // Basic piece values for evaluation
        private static readonly Dictionary<string, int> PieceValues = new Dictionary<string, int>
        {
            { "Pawn", 1 },
            { "Knight", 3 },
            { "Bishop", 3 },
            { "Rook", 5 },
            { "Queen", 9 },
            { "King", 0 }
        };
        // Position value tables for pieces
        private static readonly int[,] PawnPositionValues = new int[8, 8]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1 },
            { 2, 2, 2, 2, 2, 2, 2, 2 },
            { 3, 3, 3, 3, 3, 3, 3, 3 },
            { 4, 4, 4, 4, 4, 4, 4, 4 },
            { 5, 5, 5, 5, 5, 5, 5, 5 },
            { 6, 6, 6, 6, 6, 6, 6, 6 },
            { 7, 7, 7, 7, 7, 7, 7, 7 }
        };
        private static readonly int[,] KnightPositionValues = new int[8, 8]
        {
            { -5, -4, -3, -3, -3, -3, -4, -5 },
            { -4, -2, 0, 1, 1, 0, -2, -4 },
            { -3, 0, 1, 2, 2, 1, 0, -3 },
            { -3, 1, 2, 3, 3, 2, 1, -3 },
            { -3, 1, 2, 3, 3, 2, 1, -3 },
            { -3, 0, 1, 2, 2, 1, 0, -3 },
            { -4, -2, 0, 1, 1, 0, -2, -4 },
            { -5, -4, -3, -3, -3, -3, -4, -5 }
        };
        private static readonly int[,] BishopPositionValues = new int[8, 8]
        {
            { -4, -2, -2, -2, -2, -2, -2, -4 },
            { -2, 0, 0, 1, 1, 0, 0, -2 },
            { -2, 0, 1, 2, 2, 1, 0, -2 },
            { -2, 1, 2, 3, 3, 2, 1, -2 },
            { -2, 1, 2, 3, 3, 2, 1, -2 },
            { -2, 0, 1, 2, 2, 1, 0, -2 },
            { -2, 0, 0, 1, 1, 0, 0, -2 },
            { -4, -2, -2, -2, -2, -2, -2, -4 }
        };
        private static readonly int[,] RookPositionValues = new int[8, 8]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1 },
            { 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2, 2, 2, 2 },
            { 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        private static readonly int[,] QueenPositionValues = new int[8, 8]
        {
            { -2, -1, -1, -1, -1, -1, -1, -2 },
            { -1, 0, 0, 0, 0, 0, 0, -1 },
            { -1, 0, 1, 1, 1, 1, 0, -1 },
            { -1, 0, 1, 2, 2, 1, 0, -1 },
            { -1, 0, 1, 2, 2, 1, 0, -1 },
            { -1, 0, 1, 1, 1, 1, 0, -1 },
            { -1, 0, 0, 0, 0, 0, 0, -1 },
            { -2, -1, -1, -1, -1, -1, -1, -2 }
        };
        private static readonly int[,] KingPositionValues = new int[8, 8]
        {
            { -3, -4, -4, -5, -5, -4, -4, -3 },
            { -3, -4, -4, -5, -5, -4, -4, -3 },
            { -3, -4, -4, -5, -5, -4, -4, -3 },
            { -2, -3, -3, -4, -4, -3, -3, -2 },
            { -1, -2, -2, -3, -3, -2, -2, -1 },
            { 2, 2, 2, 0, 0, 2, 2, 2 },
            { 2, 3, 3, 4, 4, 3, 3, 2 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        public (PositionModel From, PositionModel To) GetNextMove(Game game)
        {
            var board = game.Board;
            var isWhiteTurn = game.IsWhiteTurn;
            var aiPieces = board.Squares.SelectMany(row => row)
                .Where(piece => piece != null && piece.IsWhite == isWhiteTurn)
                .ToList();
            (PositionModel From, PositionModel To) bestMove = (null, null);
            int bestValue = int.MinValue;
            foreach (var piece in aiPieces)
            {
                var validMoves = piece.GetValidMoves(board)
                    .Where(move => IsMoveSafe(board, piece, move))
                    .ToList();
                foreach (var move in validMoves)
                {
                    int moveValue = EvaluateMove(board, piece, move);
                    if (moveValue > bestValue)
                    {
                        bestValue = moveValue;
                        bestMove = (
                            new PositionModel { Row = piece.Position.Row, Col = piece.Position.Col },
                            new PositionModel { Row = move.Row, Col = move.Col }
                        );
                    }
                }
            }
            return bestMove.From != null ? bestMove : (new PositionModel { Row = 0, Col = 0 }, new PositionModel { Row = 0, Col = 0 });
        }
        // Evaluate the value of a move
        private int EvaluateMove(Board board, Piece piece, (int Row, int Col) move)
        {
            int value = 0;
            // Check if the move captures an opponent's piece
            var targetPiece = board.Squares[move.Row][move.Col];
            if (targetPiece != null && targetPiece.IsWhite != piece.IsWhite)
            {
                value += PieceValues[targetPiece.GetType().Name];
            }
            // Add positional value based on piece type
            value += GetPositionalValue(piece, move);
            // Simulate the move to check for threats
            var originalPosition = piece.Position;
            board.Squares[originalPosition.Row][originalPosition.Col] = null;
            board.Squares[move.Row][move.Col] = piece;
            piece.Position = move;

            // Check if the moved piece is now under threat
            bool isThreatened = IsPieceUnderThreat(board, piece);

            // Check for checkmate condition
            bool isCheckmate = board.IsKingInCheck(!piece.IsWhite) && !board.AreAnyMovesAvailable(!piece.IsWhite);

            // Undo the simulated move
            board.Squares[originalPosition.Row][originalPosition.Col] = piece;
            board.Squares[move.Row][move.Col] = targetPiece; // Restore target piece
            piece.Position = originalPosition;

            // If the piece is threatened, subtract its value
            if (isThreatened)
            {
                value -= PieceValues[piece.GetType().Name];
            }

            // Return a high value for checkmate
            if (isCheckmate)
            {
                return int.MaxValue; // Return a high value for checkmate
            }

            return value;
        }

        // Method to check if a piece is under threat
        private bool IsPieceUnderThreat(Board board, Piece piece)
        {
            var opponentPieces = board.Squares.SelectMany(row => row)
                .Where(p => p != null && p.IsWhite != piece.IsWhite)
                .ToList();
            foreach (var opponentPiece in opponentPieces)
            {
                var validMoves = opponentPiece.GetValidMoves(board);
                if (validMoves.Any(move => move.Row == piece.Position.Row && move.Col == piece.Position.Col))
                {
                    return true; // The piece is under threat
                }
            }
            return false; // The piece is safe
        }
        // Get positional value based on piece type and position
        private int GetPositionalValue(Piece piece, (int Row, int Col) move)
        {
            int positionalValue = 0;
            int row = move.Row;
            int col = move.Col;
            switch (piece)
            {
                case Pawn _:
                    positionalValue = PawnPositionValues[row, col];
                    break;
                case Knight _:
                    positionalValue = KnightPositionValues[row, col];
                    break;
                case Bishop _:
                    positionalValue = BishopPositionValues[row, col];
                    break;
                case Rook _:
                    positionalValue = RookPositionValues[row, col];
                    break;
                case Queen _:
                    positionalValue = QueenPositionValues[row, col];
                    break;
                case King _:
                    positionalValue = KingPositionValues[row, col];
                    break;
            }
            return positionalValue;
        }
        // Check if a move is safe
        private bool IsMoveSafe(Board board, Piece piece, (int Row, int Col) move)
        {
            var originalPosition = piece.Position;
            var capturedPiece = board.Squares[move.Row][move.Col];
            // Simulate the move
            board.Squares[originalPosition.Row][originalPosition.Col] = null;
            board.Squares[move.Row][move.Col] = piece;
            piece.Position = move;
            // Check for king safety
            bool isSafe = !board.IsKingInCheck(piece.IsWhite);
            // Undo the simulated move
            board.Squares[originalPosition.Row][originalPosition.Col] = piece;
            board.Squares[move.Row][move.Col] = capturedPiece;
            piece.Position = originalPosition;
            return isSafe;
        }
    }
}