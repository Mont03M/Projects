using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessPieces;
using ChessGame.Interfaces.Game;
using ChessGame.Structs;

namespace ChessGame.Controls.IsCheckHelper
{
    /// <summary>
    /// Provides helper methods for checking the validity of chess moves in relation to check conditions. This class contains methods to 
    /// simulate moves and filter available, attack, and special moves for a given chess piece on a cloned board state.
    /// </summary>
    public static class CheckMoveHelper
    {
        /// <summary>
        /// Filters the available, attack, and special moves of a given chess piece based on the current board state and a target piece.
        /// </summary>
        /// <param name="originalPiece">The original chess piece for which to filter moves.</param>
        /// <param name="target">The target chess piece that may be involved in the moves.</param>
        /// <param name="path">The path of locations to consider for the moves.</param>
        /// <param name="board">The current board state.</param>
        /// <returns>A tuple containing lists of available, attack, and special moves.</returns>
        public static (List<MovesAvailable>? availableMoves, List<MovesAvailable>? attackMoves,
            List<MovesAvailable>? specialMoves) GetFilteredMoves(IChessMoves originalPiece, ChessPiece target, List<ChessSquareLocation>? path, Board board)
        {
            if (originalPiece == null || board == null)
                return (null, null, null);

            // Find cloned piece by location
            var op = originalPiece as ChessPiece;
            var loc = op?.CurrentLocation;
            var clonedSquare = board.ChessSquares_?.FirstOrDefault(s => s.BoardLocation == loc.Value);

            if (clonedSquare?.ChessPiece_ is not IChessMoves clonedPiece)
                return (null, null, null);

            // Find cloned target by location
            var targetLoc = target.CurrentLocation;
            var clonedTargetSquare = board.ChessSquares_?.FirstOrDefault(s => s.BoardLocation == targetLoc);
            var clonedTarget = clonedTargetSquare?.ChessPiece_;

            // Run existing per-piece logic on the cloned piece (it mutates the clone)
            clonedPiece.GenerateIsCheckMoves(clonedTarget, path, board);

            // Capture clone lists
            List<MovesAvailable>? available = null;
            List<MovesAvailable>? attack = null;
            List<MovesAvailable>? special = null;

            // Capture the available moves, attack moves, and special moves from the cloned piece
            if (clonedPiece.AvailableMoves != null)
                available = [.. clonedPiece.AvailableMoves];

            if (clonedPiece.AttackMoves != null && (clonedPiece is not Pawn))
                attack = [.. clonedPiece.AttackMoves];

            if (clonedPiece is Pawn p && (p.AttackMoves != null || p.SpecialMoves != null))
            {
                attack = p.AttackMoves?.ToList();
                special = p.SpecialMoves?.ToList();
            }
            
            if(clonedPiece is King k && k.SpecialMoves != null)
            {
                special = [.. k.SpecialMoves];
            }

            if(clonedPiece is Rook r && r.SpecialMoves != null)
            {
                special = [.. r.SpecialMoves];
            }

            return (available, attack, special);
        }
    }
}
