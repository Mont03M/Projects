namespace ChessGame.Enums
{
    /// <summary>
    /// Represents the different types of moves that can be made in a chess game.
    /// </summary>
    public enum MoveType
    {
        NORMAL,
        EnPASSENT,
        CASTLING,
        PAWN_PROMOTION,
        CAPTURE,
        CHECK,
        CHECKMATE
    }
}
