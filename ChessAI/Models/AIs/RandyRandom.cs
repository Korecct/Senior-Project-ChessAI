using static ChessAI.Controllers.HomeController;

namespace ChessAI.Models.AIs
{
    // Randy Random is very random
    
    // Do not modify, make a copy of this file and rename the AI

    public class RandyRandom : IAIPlayer
    {
        private static readonly Random _random = new Random();

        public string Name => "Randy Random";

        public (PositionModel From, PositionModel To) GetNextMove(Game game)
        {
            var board = game.Board;              // Get the current state of the chessboard
            var isWhiteTurn = game.IsWhiteTurn;  // Check whose turn it is

            // Get all pieces belonging to the current player (AI)
            var aiPieces = board.Squares.SelectMany(row => row)
                                       .Where(piece => piece != null && piece.IsWhite == isWhiteTurn)
                                       .ToList();

            // Shuffles the list of AI pieces randomly
            var shuffledPieces = aiPieces.OrderBy(p => _random.Next()).ToList();

            // Then iterates through the shuffled list of pieces to find a valid move
            foreach (var piece in shuffledPieces)
            {
                // Get all valid moves for the piece that are also safe (like do not put the AI's king in check)
                var validMoves = piece.GetValidMoves(board)
                                      .Where(move => IsMoveSafe(board, piece, move))
                                      .ToList();

                // If any valid moves are found, randomly select one and return it
                if (validMoves.Count != 0)
                {
                    var move = validMoves[_random.Next(validMoves.Count)]; // Randomly pick a valid move
                    return (
                        new PositionModel { Row = piece.Position.Row, Col = piece.Position.Col }, // From position
                        new PositionModel { Row = move.Row, Col = move.Col }  // To position
                    );
                }
            }

            // If no valid moves are found, return a default move
            return (new PositionModel { Row = 0, Col = 0 }, new PositionModel { Row = 0, Col = 0 });
        }

        // Check if a move is safe
        private bool IsMoveSafe(Board board, Piece piece, (int Row, int Col) move)
        {
            // Store original position of the piece
            var originalPosition = piece.Position;
            // Get any piece that might be in the target position (to simulate capturing)
            var capturedPiece = board.Squares[move.Row][move.Col];

            // Simulate the move by updating the board and piece position
            board.Squares[originalPosition.Row][originalPosition.Col] = null;
            board.Squares[move.Row][move.Col] = piece;
            piece.Position = move;

            // Check if the move results in the AIs king being in check
            bool isSafe = !board.IsKingInCheck(piece.IsWhite);

            // Undo the simulated move to restore the board to its original state
            board.Squares[originalPosition.Row][originalPosition.Col] = piece;
            board.Squares[move.Row][move.Col] = capturedPiece;
            piece.Position = originalPosition;

            return isSafe;
        }
    }
}
