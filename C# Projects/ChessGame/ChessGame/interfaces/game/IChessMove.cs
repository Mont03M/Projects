using ChessGame.Structs;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Defines the interface for a chess move, which includes a property for the move's location on the chessboard.
    /// </summary>
    public interface IChessMove
    {
        /// <summary>
        /// Gets or sets the location of the move on the chessboard.
        /// </summary>
        ChessSquareLocation Move { get; set; }
    }
}
