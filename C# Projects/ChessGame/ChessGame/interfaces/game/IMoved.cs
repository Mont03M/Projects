using ChessGame.Structs;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Interface representing a chess piece that has moved. It provides properties to track whether the piece has moved, 
    /// its starting location, and a method to mark the piece as moved.
    /// </summary>
    public interface IMoved
    {
        /// <summary>
        /// Gets or sets a value indicating whether the chess piece has moved from its starting location.
        /// </summary>
        public bool ChessPieceMoved { get; set; }

        /// <summary>
        /// Gets or sets the starting location of the chess piece before it moved.
        /// </summary>
        public ChessSquareLocation StartLocation { get; set; }

        /// <summary>
        /// Marks the chess piece as having moved, updating the ChessPieceMoved property to true.
        /// </summary>
        public void Moved();
    }
}
