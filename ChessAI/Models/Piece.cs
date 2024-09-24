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
            int direction = IsWhite ? -1 : 1; // White moves up, black moves down

            int forwardRow = Position.Row + direction;

            // Forward movement
            if (board.IsWithinBounds(forwardRow, Position.Col) && board.IsEmpty(forwardRow, Position.Col))
            {
                moves.Add((forwardRow, Position.Col));

                // First move can be two squares forward
                if ((IsWhite && Position.Row == 6) || (!IsWhite && Position.Row == 1))
                {
                    int doubleForwardRow = Position.Row + 2 * direction;
                    if (board.IsEmpty(doubleForwardRow, Position.Col))
                    {
                        moves.Add((doubleForwardRow, Position.Col));
                    }
                }
            }

            // Captures
            int[] captureCols = { Position.Col - 1, Position.Col + 1 };
            foreach (int col in captureCols)
            {
                if (board.IsWithinBounds(forwardRow, col) && board.IsEnemyPiece(forwardRow, col, IsWhite))
                {
                    moves.Add((forwardRow, col));
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

            // Note: Castling not implemented here

            return moves;
        }
    }


}
