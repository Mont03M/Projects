using ChessGame.ChessGameMoves.Moves;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Interface for chess pieces that have special moves. It defines a property to hold a list of special moves available to the piece.
    /// </summary>
    public interface IPieceSpecialMove
    {
        /// <summary>
        /// Gets or sets the list of special moves available to the chess piece. This property can be null if there are no special moves defined for the piece.
        /// </summary>
        public List<MovesAvailable>? SpecialMoves { get; set; }
    }
}
