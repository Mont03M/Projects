namespace ChessGame.Enums
{
    /// <summary>
    /// Represents the reasons for a draw in a chess game.
    /// </summary>
    public enum DrawReason
    {
        None,
        Stalemate,
        InsufficientMaterial,
        ThreefoldRepetition, 
        FiftyMoveRule,
        SeventyFiveMoveRule,
        Agreement,
        DeadPosition
    }
}
