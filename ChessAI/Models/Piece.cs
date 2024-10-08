namespace ChessAI.Models
{
    [Serializable]
    public abstract class Piece
    {
        public bool IsWhite { get; set; }
        public (int Row, int Col) Position { get; set; }

        public abstract List<(int Row, int Col)> GetValidMoves(Board board);

        public virtual List<(int Row, int Col)> GetValidMovesIgnoringCheck(Board board)
        {
            return GetValidMoves(board);
        }
    }

    [Serializable]
    public class Pawn : Piece
    {
        public bool HasMoved { get; set; } = false;
        public bool EnPassantEligible { get; set; } = false;

        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int direction = IsWhite ? -1 : 1; // White moves up, black moves down

            int forwardRow = Position.Row + direction;

            // Forward movement
            if (board.IsWithinBounds(forwardRow, Position.Col) && board.IsEmpty(forwardRow, Position.Col))
            {
                moves.Add((forwardRow, Position.Col));

                // First move can be two squares forward
                if (!HasMoved)
                {
                    int doubleForwardRow = Position.Row + 2 * direction;
                    if (board.IsWithinBounds(doubleForwardRow, Position.Col) && board.IsEmpty(doubleForwardRow, Position.Col) && board.IsEmpty(forwardRow, Position.Col))
                    {
                        moves.Add((doubleForwardRow, Position.Col));
                    }
                }
            }

            // Captures
            int[] captureCols = { Position.Col - 1, Position.Col + 1 };
            foreach (int col in captureCols)
            {
                if (board.IsWithinBounds(forwardRow, col))
                {
                    // Normal capture
                    if (board.IsEnemyPiece(forwardRow, col, IsWhite))
                    {
                        moves.Add((forwardRow, col));
                    }
                    // En Passant capture
                    if (board.IsEmpty(forwardRow, col) && board.Squares[Position.Row][col] is Pawn adjacentPawn)
                    {
                        if (adjacentPawn.IsWhite != IsWhite && adjacentPawn.EnPassantEligible)
                        {
                            moves.Add((forwardRow, col));
                        }
                    }
                }
            }

            return moves;
        }
    }

    [Serializable]
    public class Rook : Piece
    {
        public bool HasMoved { get; set; } = false;

        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int[] rowDirections = { -1, 1, 0, 0 };
            int[] colDirections = { 0, 0, -1, 1 };

            for (int dir = 0; dir < 4; dir++)
            {
                int rowDir = rowDirections[dir];
                int colDir = colDirections[dir];

                for (int i = 1; i < 8; i++)
                {
                    int newRow = Position.Row + i * rowDir;
                    int newCol = Position.Col + i * colDir;

                    if (board.IsWithinBounds(newRow, newCol))
                    {
                        if (board.IsEmpty(newRow, newCol))
                        {
                            moves.Add((newRow, newCol));
                        }
                        else
                        {
                            if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                            {
                                moves.Add((newRow, newCol));
                            }
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moves;
        }
    }

    [Serializable]
    public class Knight : Piece
    {
        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int[] rowOffsets = { -2, -1, 1, 2 };
            int[] colOffsets = { -2, -1, 1, 2 };

            foreach (int rowOffset in rowOffsets)
            {
                foreach (int colOffset in colOffsets)
                {
                    if (Math.Abs(rowOffset) != Math.Abs(colOffset))
                    {
                        int newRow = Position.Row + rowOffset;
                        int newCol = Position.Col + colOffset;
                        if (board.IsWithinBounds(newRow, newCol))
                        {
                            if (board.IsEmpty(newRow, newCol) || board.IsEnemyPiece(newRow, newCol, IsWhite))
                            {
                                moves.Add((newRow, newCol));
                            }
                        }
                    }
                }
            }

            return moves;
        }
    }

    [Serializable]
    public class Bishop : Piece
    {
        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int[] rowDirections = { -1, -1, 1, 1 };
            int[] colDirections = { -1, 1, -1, 1 };

            for (int dir = 0; dir < 4; dir++)
            {
                int rowDir = rowDirections[dir];
                int colDir = colDirections[dir];

                for (int i = 1; i < 8; i++)
                {
                    int newRow = Position.Row + i * rowDir;
                    int newCol = Position.Col + i * colDir;

                    if (board.IsWithinBounds(newRow, newCol))
                    {
                        if (board.IsEmpty(newRow, newCol))
                        {
                            moves.Add((newRow, newCol));
                        }
                        else
                        {
                            if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                            {
                                moves.Add((newRow, newCol));
                            }
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return moves;
        }
    }

    [Serializable]
    public class Queen : Piece
    {
        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();

            // Queen combines Rook and Bishop moves
            var rook = new Rook { IsWhite = this.IsWhite, Position = this.Position };
            var bishop = new Bishop { IsWhite = this.IsWhite, Position = this.Position };

            moves.AddRange(rook.GetValidMoves(board));
            moves.AddRange(bishop.GetValidMoves(board));

            return moves;
        }
    }

    [Serializable]
    public class King : Piece
    {
        public bool HasMoved { get; set; } = false;

        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int[] offsets = { -1, 0, 1 };

            foreach (int rowOffset in offsets)
            {
                foreach (int colOffset in offsets)
                {
                    if (rowOffset != 0 || colOffset != 0)
                    {
                        int newRow = Position.Row + rowOffset;
                        int newCol = Position.Col + colOffset;
                        if (board.IsWithinBounds(newRow, newCol))
                        {
                            if (board.IsEmpty(newRow, newCol) || board.IsEnemyPiece(newRow, newCol, IsWhite))
                            {
                                moves.Add((newRow, newCol));
                            }
                        }
                    }
                }
            }

            // Castling
            if (!HasMoved && !board.IsKingInCheck(IsWhite))
            {
                // Kingside castling
                if (CanCastle(board, kingside: true))
                {
                    moves.Add((Position.Row, Position.Col + 2));
                }
                // Queenside castling
                if (CanCastle(board, kingside: false))
                {
                    moves.Add((Position.Row, Position.Col - 2));
                }
            }

            return moves;
        }

        private bool CanCastle(Board board, bool kingside)
        {
            int row = Position.Row;
            int col = Position.Col;
            int direction = kingside ? 1 : -1;
            int rookCol = kingside ? 7 : 0;

            // Check if the squares between king and rook are empty
            int startCol = Math.Min(col, rookCol) + 1;
            int endCol = Math.Max(col, rookCol) - 1;

            for (int c = startCol; c <= endCol; c++)
            {
                if (!board.IsEmpty(row, c))
                {
                    return false;
                }
            }

            // Check if the rook exists and has not moved
            if (board.Squares[row][rookCol] is Rook rook && rook.IsWhite == IsWhite && !rook.HasMoved)
            {
                // Check that the squares the king passes through are not under attack
                for (int c = col + direction; c != col + 2 * direction; c += direction)
                {
                    if (board.IsSquareUnderAttack(row, c, !IsWhite))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }

        public override List<(int Row, int Col)> GetValidMovesIgnoringCheck(Board board)
        {
            // Exclude castling to avoid infinite recursion
            var moves = new List<(int Row, int Col)>();
            int[] offsets = { -1, 0, 1 };

            foreach (int rowOffset in offsets)
            {
                foreach (int colOffset in offsets)
                {
                    if (rowOffset != 0 || colOffset != 0)
                    {
                        int newRow = Position.Row + rowOffset;
                        int newCol = Position.Col + colOffset;
                        if (board.IsWithinBounds(newRow, newCol))
                        {
                            if (board.IsEmpty(newRow, newCol) || board.IsEnemyPiece(newRow, newCol, IsWhite))
                            {
                                moves.Add((newRow, newCol));
                            }
                        }
                    }
                }
            }

            return moves;
        }
    }
}