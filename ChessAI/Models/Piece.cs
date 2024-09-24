namespace ChessAI.Models
{
    [Serializable]
    public abstract class Piece
    {
        public bool IsWhite { get; set; }
        public (int Row, int Col) Position { get; set; }

        public abstract List<(int Row, int Col)> GetValidMoves(Board board);
    }

    [Serializable]
    public class Pawn : Piece
    {
        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            // Move forward
            int newRow = Position.Row + direction;
            if (board.IsEmpty(newRow, Position.Col))
            {
                moves.Add((newRow, Position.Col));

                // Double move from starting position
                if (Position.Row == startRow && board.IsEmpty(newRow + direction, Position.Col))
                {
                    moves.Add((newRow + direction, Position.Col));
                }
            }

            // Captures
            foreach (int colOffset in new[] { -1, 1 })
            {
                int newCol = Position.Col + colOffset;
                if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                {
                    moves.Add((newRow, newCol));
                }
            }

            return moves;
        }
    }

    [Serializable]
    public class Rook : Piece
    {
        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            // Move forward
            int newRow = Position.Row + direction;
            if (board.IsEmpty(newRow, Position.Col))
            {
                moves.Add((newRow, Position.Col));

                // Double move from starting position
                if (Position.Row == startRow && board.IsEmpty(newRow + direction, Position.Col))
                {
                    moves.Add((newRow + direction, Position.Col));
                }
            }

            // Captures
            foreach (int colOffset in new[] { -1, 1 })
            {
                int newCol = Position.Col + colOffset;
                if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                {
                    moves.Add((newRow, newCol));
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
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            // Move forward
            int newRow = Position.Row + direction;
            if (board.IsEmpty(newRow, Position.Col))
            {
                moves.Add((newRow, Position.Col));

                // Double move from starting position
                if (Position.Row == startRow && board.IsEmpty(newRow + direction, Position.Col))
                {
                    moves.Add((newRow + direction, Position.Col));
                }
            }

            // Captures
            foreach (int colOffset in new[] { -1, 1 })
            {
                int newCol = Position.Col + colOffset;
                if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                {
                    moves.Add((newRow, newCol));
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
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            // Move forward
            int newRow = Position.Row + direction;
            if (board.IsEmpty(newRow, Position.Col))
            {
                moves.Add((newRow, Position.Col));

                // Double move from starting position
                if (Position.Row == startRow && board.IsEmpty(newRow + direction, Position.Col))
                {
                    moves.Add((newRow + direction, Position.Col));
                }
            }

            // Captures
            foreach (int colOffset in new[] { -1, 1 })
            {
                int newCol = Position.Col + colOffset;
                if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                {
                    moves.Add((newRow, newCol));
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
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            // Move forward
            int newRow = Position.Row + direction;
            if (board.IsEmpty(newRow, Position.Col))
            {
                moves.Add((newRow, Position.Col));

                // Double move from starting position
                if (Position.Row == startRow && board.IsEmpty(newRow + direction, Position.Col))
                {
                    moves.Add((newRow + direction, Position.Col));
                }
            }

            // Captures
            foreach (int colOffset in new[] { -1, 1 })
            {
                int newCol = Position.Col + colOffset;
                if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                {
                    moves.Add((newRow, newCol));
                }
            }

            return moves;
        }
    }

    [Serializable]
    public class King : Piece
    {
        public override List<(int Row, int Col)> GetValidMoves(Board board)
        {
            var moves = new List<(int Row, int Col)>();
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            // Move forward
            int newRow = Position.Row + direction;
            if (board.IsEmpty(newRow, Position.Col))
            {
                moves.Add((newRow, Position.Col));

                // Double move from starting position
                if (Position.Row == startRow && board.IsEmpty(newRow + direction, Position.Col))
                {
                    moves.Add((newRow + direction, Position.Col));
                }
            }

            // Captures
            foreach (int colOffset in new[] { -1, 1 })
            {
                int newCol = Position.Col + colOffset;
                if (board.IsEnemyPiece(newRow, newCol, IsWhite))
                {
                    moves.Add((newRow, newCol));
                }
            }

            return moves;
        }
    }

}
