namespace ChessAI.Models
{
    [Serializable]
    public class Game
    {
        public Board Board { get; set; } = new Board();
        public bool IsWhiteTurn { get; set; } = true;

        public bool MakeMove((int Row, int Col) from, (int Row, int Col) to)
        {
            var piece = Board.Squares[from.Row, from.Col];
            if (piece == null || piece.IsWhite != IsWhiteTurn)
            {
                // Not the player's turn or no piece at the position
                return false;
            }

            var success = Board.MovePiece(from, to, IsWhiteTurn);
            if (success)
            {
                IsWhiteTurn = !IsWhiteTurn;
            }
            return success;
        }

    }

}
