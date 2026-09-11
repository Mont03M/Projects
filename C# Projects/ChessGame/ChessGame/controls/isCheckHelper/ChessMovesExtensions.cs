using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessPieces;
using ChessGame.Interfaces.Game;
using ChessGame.Structs;

namespace ChessGame.Controls.IsCheckHelper
{
    /// <summary>
    /// Provides extension methods for the IChessMoves interface to 
    /// filter available, attack, and special moves based on the current board state and a target piece. 
    /// These methods utilize the CheckMoveHelper to simulate moves and return the filtered results.
    /// </summary>
    public static class ChessMovesExtensions
    {
        /// <summary>
        /// Filters the available moves for a given chess piece based on the current board state and a target piece.
        /// </summary>
        /// <param name="piece">The chess piece for which to filter available moves.</param>
        /// <param name="target">The target chess piece that may be involved in the moves.</param>
        /// <param name="path">The path of locations to consider for the moves.</param>
        /// <param name="board">The current board state.</param>
        /// <returns>A list of filtered available moves.</returns>
        public static List<MovesAvailable>? GetFilteredAvailableMoves(this IChessMoves piece, ChessPiece target, List<ChessSquareLocation>? path, Board board)
        {
            var result = CheckMoveHelper.GetFilteredMoves(piece, target, path, board);
            return result.availableMoves;
        }

        /// <summary>
        /// Filters the attack moves for a given chess piece based on the current board state and a target piece.
        /// </summary>
        /// <param name="piece">The chess piece for which to filter attack moves.</param>
        /// <param name="target">The target chess piece that may be involved in the moves.</param>
        /// <param name="path">The path of locations to consider for the moves.</param>
        /// <param name="board">The current board state.</param>
        /// <returns>A list of filtered attack moves.</returns>
        public static List<MovesAvailable>? GetFilteredAttackMoves(this IChessMoves piece, ChessPiece target, List<ChessSquareLocation>? path, Board board)
        {
            var result = CheckMoveHelper.GetFilteredMoves(piece, target, path, board);
            return result.attackMoves;
        }
        
        /// <summary>
        /// Filters the special moves for a given chess piece based on the current board state and a target piece.
        /// </summary>
        /// <param name="piece">The chess piece for which to filter special moves.</param>
        /// <param name="target">The target chess piece that may be involved in the moves.</param>
        /// <param name="path">The path of locations to consider for the moves.</param>
        /// <param name="board">The current board state.</param>
        /// <returns>A list of filtered special moves.</returns> 
        public static List<MovesAvailable>? GetFilteredSpecialMoves(this IChessMoves piece, ChessPiece target, List<ChessSquareLocation>? path, Board board)
        {
            var result = CheckMoveHelper.GetFilteredMoves(piece, target, path, board);
            return result.specialMoves;
        }
    }
}
