using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.PeekMoves;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Interface representing a collection of chess moves lists, including available moves, attack moves, and all available moves (peek).
    /// </summary>
    public interface IChessMovesLists
    {
        /// <summary>
        /// Gets or sets the list of available moves.
        /// </summary>
        public List<MovesAvailable>? AvailableMoves { get; set; }

        /// <summary>
        /// Gets or sets the list of attack moves.
        /// </summary>
        public List<MovesAvailable>? AttackMoves { get; set; }

        /// <summary>
        /// Gets or sets the list of all available moves (peek).
        /// </summary>
        public List<Peek>? AllAvailableMoves { get; set; }

    }
}
