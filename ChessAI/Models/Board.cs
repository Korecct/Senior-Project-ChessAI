using System.ComponentModel;

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
            /*
             * [0,0] is top-left corner from white's perspective(A8)
             * [0,7] is top-right corner from white's perspective (H8)
             * [7,0] is bottom-left corner from white's perspective (A1) 
             * [7,7] is bottom-right corner from white's perspective (H1)
            */
            // Initialize pawns
            for (int i = 0; i < 8; i++)
            {
                Squares[6, i] = new Pawn { IsWhite = true, Position = (6, i) };
                Squares[1, i] = new Pawn { IsWhite = false, Position = (1, i) }; 
            }

            // Initialize rooks
             Squares[7, 0] = new Rook { IsWhite = true, Position = (7, 0) }; //A1
             Squares[7, 7] = new Rook { IsWhite = true, Position = (7, 7) }; //H1
             Squares[0, 0] = new Rook { IsWhite = false, Position = (0, 0) };//A8
             Squares[0, 7] = new Rook { IsWhite = false, Position = (0, 7) };//H8

             // Initialize knights
             Squares[7, 1] = new Knight { IsWhite = true, Position = (7, 1) };//B1
             Squares[7, 6] = new Knight { IsWhite = true, Position = (7, 6) };//G1
             Squares[0, 1] = new Knight { IsWhite = false, Position = (0, 1) };//B8
             Squares[0, 6] = new Knight { IsWhite = false, Position = (0, 6) };//G8

             // Initialize bishops
             Squares[7, 2] = new Bishop { IsWhite = true, Position = (7, 2) };//C1
             Squares[7, 5] = new Bishop { IsWhite = true, Position = (7, 5) };//F1
             Squares[0, 2] = new Bishop { IsWhite = false, Position = (0, 2) };//C8
             Squares[0, 5] = new Bishop { IsWhite = false, Position = (0, 5) };//F8

             // Initialize queens
             Squares[7, 3] = new Queen { IsWhite = true, Position = (7, 3) };//D1
             Squares[0, 3] = new Queen { IsWhite = false, Position = (0, 3) };//D8

             // Initialize kings
             Squares[7, 4] = new King { IsWhite = true, Position = (7, 4) };//E1
             Squares[0, 4] = new King { IsWhite = false, Position = (0, 4) };//E8
           
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
        public bool isKingInCheck(bool isWhite)
        {
            (int Row, int Col) kingPosition = findKingPosition(isWhite);
            foreach (var piece in Squares) //For every piece on a square
            {
                if (piece != null && piece.IsWhite != isWhite)//If there is a piece that is not under the control of the player whose current turn it is, then
                {
                    var validMoves = piece.GetValidMoves(this); //Get 
                    if (validMoves.Any(move => move.Row == kingPosition.Row && move.Col == kingPosition.Col)) //Changed m to move
                    {
                        return true;
                    }
                }
            }
            return false;
            
        }


        public (int Row, int Col) findKingPosition(bool isWhite)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (Squares[row, col] is King king && king.IsWhite == isWhite)
                    {
                        return (row, col);
                    }
                }
            }
            throw new Exception("King not found!");
        }
        public bool AreAnyEscapeMovesAvailable(bool isWhite)
        {
            var kingPosition = findKingPosition(isWhite);
            var king = Squares[kingPosition.Row, kingPosition.Col] as King;

            // Get all valid moves for the king
            var validKingMoves = king.GetValidMoves(this);
            foreach (var move in validKingMoves)
            {
                // Store original position
                var originalPosition = king.Position;

                // Simulate the move
                Squares[move.Row, move.Col] = king;
                Squares[kingPosition.Row, kingPosition.Col] = null;
                king.Position = move;

                // Check if the king is in check after the move
                if (!isKingInCheck(isWhite))
                {
                    // Restore original position
                    Squares[kingPosition.Row, kingPosition.Col] = king;
                    Squares[move.Row, move.Col] = null;
                    king.Position = originalPosition;
                    return true; // Found a valid escape move
                }

                // Restore original position
                Squares[kingPosition.Row, kingPosition.Col] = king;
                Squares[move.Row, move.Col] = null;
                king.Position = originalPosition;
            }

            return false; // No escape moves available
        }
        public bool IsStalemate(bool isWhite)
        {
            // First, check if the king is in check. If it is, it's not a stalemate.
            if (isKingInCheck(isWhite))
            {
                return false;
            }

            // Check if there are only kings left for both sides
            int whitePiecesCount = 0;
            int blackPiecesCount = 0;

            foreach (var piece in Squares)
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

            // If only kings are left, it's a stalemate
            if (whitePiecesCount == 1 && blackPiecesCount == 1)
            {
                return true;
            }

            // Check if there are any legal moves available
            return !AreAnyMovesAvailable(isWhite);
        }

        public bool AreAnyMovesAvailable(bool isWhite)
        {
            // Iterate through all squares on the board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Check if the piece belongs to the player
                    if (Squares[row, col] is Piece piece && piece.IsWhite == isWhite)
                    {
                        // Get valid moves for the piece
                        var validMoves = piece.GetValidMoves(this);
                        foreach (var move in validMoves)
                        {
                            // Store original position
                            var originalPosition = piece.Position;

                            // Simulate the move
                            Squares[move.Row, move.Col] = piece;
                            Squares[row, col] = null;
                            piece.Position = move;

                            // Check if the king is in check after the move
                            if (!isKingInCheck(isWhite))
                            {
                                // Restore original position
                                Squares[row, col] = piece;
                                Squares[move.Row, move.Col] = null;
                                piece.Position = originalPosition;
                                return true; // Found a valid move
                            }

                            // Restore original position
                            Squares[row, col] = piece;
                            Squares[move.Row, move.Col] = null;
                            piece.Position = originalPosition;
                        }
                    }
                }
            }
            return false; // No moves available for any of the player's pieces
        }




        public bool checkEnPassant(bool isWhite, int currentPawnCol, int passingRow)
        {
            //On row 3 (for white) or row 4 (for black), check that a piece that is in the adjacent columns of a pawn of the appropiate color is a pawn of the opposite color
            int[] adjacentPawnCol = { currentPawnCol - 1, currentPawnCol + 1 };
            switch (passingRow)
            {
                case 3: //Check if white can perform En Passant 
                {
                        foreach (int col in adjacentPawnCol)
                        {
                            if (col >= 0 && col <= 7)
                            {
                                if (Squares[passingRow, col] is Pawn pawn && pawn.IsWhite == isWhite)
                                {
                                    return true;
                                }
                            }
                        }
                            break;
                }
                case 4://Check if black can perform En Passant
                {
                        foreach (int col in adjacentPawnCol)
                        {
                            if (col >= 0 && col <= 7)
                            {
                                if (Squares[passingRow, col] is Pawn pawn && pawn.IsWhite == isWhite)
                                {
                                    return true;
                                }
                            }
                        }
                        break;
                }
                default:
                {
                        return false;
                   // break;
                }
            }
            return false;
           // throw new Exception("Error with EnPassant check.");
        }

    }

}
