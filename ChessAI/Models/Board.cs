namespace ChessAI.Models
{
    [Serializable]
    public class Board
    {
        public Piece[,] Squares { get; set; } = new Piece[8, 8];

        public Board()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Initialize pawns
            for (int i = 0; i < 8; i++)
            {
                Squares[6, i] = new Pawn { IsWhite = true, Position = (6, i) };
                Squares[1, i] = new Pawn { IsWhite = false, Position = (1, i) };
            }

            // Initialize rooks
            Squares[7, 0] = new Rook { IsWhite = true, Position = (7, 0) };
            Squares[7, 7] = new Rook { IsWhite = true, Position = (7, 7) };
            Squares[0, 0] = new Rook { IsWhite = false, Position = (0, 0) };
            Squares[0, 7] = new Rook { IsWhite = false, Position = (0, 7) };

            // Initialize knights
            Squares[7, 1] = new Knight { IsWhite = true, Position = (7, 1) };
            Squares[7, 6] = new Knight { IsWhite = true, Position = (7, 6) };
            Squares[0, 1] = new Knight { IsWhite = false, Position = (0, 1) };
            Squares[0, 6] = new Knight { IsWhite = false, Position = (0, 6) };

            // Initialize bishops
            Squares[7, 2] = new Bishop { IsWhite = true, Position = (7, 2) };
            Squares[7, 5] = new Bishop { IsWhite = true, Position = (7, 5) };
            Squares[0, 2] = new Bishop { IsWhite = false, Position = (0, 2) };
            Squares[0, 5] = new Bishop { IsWhite = false, Position = (0, 5) };

            // Initialize queens
            Squares[7, 3] = new Queen { IsWhite = true, Position = (7, 3) };
            Squares[0, 3] = new Queen { IsWhite = false, Position = (0, 3) };

            // Initialize kings
            Squares[7, 4] = new King { IsWhite = true, Position = (7, 4) };
            Squares[0, 4] = new King { IsWhite = false, Position = (0, 4) };
        }

        public bool IsEmpty(int row, int col)
        {
            return IsWithinBounds(row, col) && Squares[row, col] == null;
        }

        public bool IsEnemyPiece(int row, int col, bool isWhite)
        {
            return IsWithinBounds(row, col) && Squares[row, col]?.IsWhite == !isWhite;
        }

        public bool IsWithinBounds(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        public bool MovePiece((int Row, int Col) from, (int Row, int Col) to, bool isWhiteTurn)
        {
            var piece = Squares[from.Row, from.Col];
            if (piece == null || piece.IsWhite != isWhiteTurn)
                return false;

            var validMoves = piece.GetValidMoves(this);
            if (validMoves.Any(m => m == to))
            {
                // Move piece
                Squares[to.Row, to.Col] = piece;
                Squares[from.Row, from.Col] = null;
                piece.Position = to;
                return true;
            }
            return false;
        }
    }

}
