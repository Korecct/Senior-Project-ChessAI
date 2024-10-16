namespace ChessAI.Models
{
    [Serializable]
    public class Board
    {
        public Piece[][] Squares { get; set; } = new Piece[8][];

        public Board()
        {
            // Initialize each row in the jagged array
            for (int i = 0; i < 8; i++)
            {
                Squares[i] = new Piece[8];
            }
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            /*
             * [0,0] is top-left corner from white's perspective(A8)
             * [0,7] is top-right corner from white's perspective (H8)
             * [7,0] is bottom-left corner from white's perspective (A1) 
             * [7,7] is bottom-right corner from white's perspective (H1)
            */
            // Initialize pawns
            for (int i = 0; i < 8; i++)
            {
                Squares[6][i] = new Pawn { IsWhite = true, Position = (6, i) };
                Squares[1][i] = new Pawn { IsWhite = false, Position = (1, i) };
            }

            // Initialize rooks
            Squares[7][0] = new Rook { IsWhite = true, Position = (7, 0) };//A1
            Squares[7][7] = new Rook { IsWhite = true, Position = (7, 7) };//H1
            Squares[0][0] = new Rook { IsWhite = false, Position = (0, 0) };//A8
            Squares[0][7] = new Rook { IsWhite = false, Position = (0, 7) };//H8

            // Initialize knights
            Squares[7][1] = new Knight { IsWhite = true, Position = (7, 1) };//B1
            Squares[7][6] = new Knight { IsWhite = true, Position = (7, 6) };//G1
            Squares[0][1] = new Knight { IsWhite = false, Position = (0, 1) };//B8
            Squares[0][6] = new Knight { IsWhite = false, Position = (0, 6) };//G8

            // Initialize bishops
            Squares[7][2] = new Bishop { IsWhite = true, Position = (7, 2) };//C1
            Squares[7][5] = new Bishop { IsWhite = true, Position = (7, 5) };//F1
            Squares[0][2] = new Bishop { IsWhite = false, Position = (0, 2) };//C8
            Squares[0][5] = new Bishop { IsWhite = false, Position = (0, 5) };//F8

            // Initialize queens
            Squares[7][3] = new Queen { IsWhite = true, Position = (7, 3) };//D1
            Squares[0][3] = new Queen { IsWhite = false, Position = (0, 3) };//D8

            // Initialize kings
            Squares[7][4] = new King { IsWhite = true, Position = (7, 4) };//E1
            Squares[0][4] = new King { IsWhite = false, Position = (0, 4) };//E8
        }

        public bool IsEmpty(int row, int col)
        {
            return IsWithinBounds(row, col) && Squares[row][col] == null;
        }

        public bool IsEnemyPiece(int row, int col, bool isWhite)
        {
            return IsWithinBounds(row, col) && Squares[row][col]?.IsWhite == !isWhite;
        }

        public bool IsWithinBounds(int row, int col)
        {
            return row >= 0 && row < 8 && col >= 0 && col < 8;
        }

        public bool IsKingInCheck(bool isWhite)
        {
            (int Row, int Col) kingPosition = FindKingPosition(isWhite);
            return IsSquareUnderAttack(kingPosition.Row, kingPosition.Col, !isWhite);
        }

        public bool IsSquareUnderAttack(int row, int col, bool byWhite)
        {
            foreach (var pieceRow in Squares)
            {
                foreach (var piece in pieceRow)
                {
                    if (piece != null && piece.IsWhite == byWhite)
                    {
                        var validMoves = piece.GetValidMovesIgnoringCheck(this);
                        if (validMoves.Any(move => move.Row == row && move.Col == col))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public (int Row, int Col) FindKingPosition(bool isWhite)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (Squares[row][col] is King king && king.IsWhite == isWhite)
                    {
                        return (row, col);
                    }
                }
            }
            throw new Exception("King not found!");
        }

        public bool IsStalemate(bool isWhite)
        {
            // First, check if the king is in check. If it is, it's not a stalemate.
            if (IsKingInCheck(isWhite))
            {
                return false;
            }

            // Check if there are only kings left for both sides
            int whitePiecesCount = 0;
            int blackPiecesCount = 0;

            foreach (var row in Squares)
            {
                if (row != null)
                {
                    foreach (var piece in row)
                    {
                        if (piece != null)
                        {
                            if (piece.IsWhite)
                            {
                                whitePiecesCount++;
                            }
                            else
                            {
                                blackPiecesCount++;
                            }
                        }
                    }
                }
            }

            return !AreAnyMovesAvailable(isWhite);
        }

        public bool IsInsufficientMaterial()
        {
            List<Piece> whitePieces = new List<Piece>();
            List<Piece> blackPieces = new List<Piece>();

            foreach (var row in Squares)
            {
                foreach (var piece in row)
                {
                    if (piece != null)
                    {
                        if (piece.IsWhite)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }
                    }
                }
            }

            // Remove kings from the lists
            whitePieces.RemoveAll(p => p is King);
            blackPieces.RemoveAll(p => p is King);

            // King vs King
            if (whitePieces.Count == 0 && blackPieces.Count == 0)
            {
                return true;
            }

            // King and Bishop or Knight vs King
            if ((whitePieces.Count == 1 && (whitePieces[0] is Bishop || whitePieces[0] is Knight) && blackPieces.Count == 0) ||
                (blackPieces.Count == 1 && (blackPieces[0] is Bishop || blackPieces[0] is Knight) && whitePieces.Count == 0))
            {
                return true;
            }

            // King and Bishop vs. King and Bishop with bishops on the same color
            if (whitePieces.Count == 1 && blackPieces.Count == 1 &&
                whitePieces[0] is Bishop && blackPieces[0] is Bishop)
            {
                var whiteBishop = (Bishop)whitePieces[0];
                var blackBishop = (Bishop)blackPieces[0];

                bool whiteBishopOnLightSquare = (whiteBishop.Position.Row + whiteBishop.Position.Col) % 2 == 0;
                bool blackBishopOnLightSquare = (blackBishop.Position.Row + blackBishop.Position.Col) % 2 == 0;

                if (whiteBishopOnLightSquare == blackBishopOnLightSquare)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AreAnyMovesAvailable(bool isWhite)
        {
            // Iterate through all squares on the board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Check if the piece belongs to the player
                    if (Squares[row][col] is Piece piece && piece.IsWhite == isWhite)
                    {
                        // Get valid moves for the piece
                        var validMoves = piece.GetValidMoves(this);
                        foreach (var move in validMoves)
                        {
                            // Simulate the move
                            var originalPosition = piece.Position;
                            var capturedPiece = Squares[move.Row][move.Col];

                            Squares[originalPosition.Row][originalPosition.Col] = null;
                            Squares[move.Row][move.Col] = piece;
                            piece.Position = move;

                            // Check if own king is in check
                            bool isInCheck = IsKingInCheck(isWhite);

                            // Undo the move
                            Squares[originalPosition.Row][originalPosition.Col] = piece;
                            Squares[move.Row][move.Col] = capturedPiece;
                            piece.Position = originalPosition;

                            if (!isInCheck)
                            {
                                return true; // Found a valid move
                            }
                        }
                    }
                }
            }
            return false; // No moves available for any of the player's pieces
        }
    }
}