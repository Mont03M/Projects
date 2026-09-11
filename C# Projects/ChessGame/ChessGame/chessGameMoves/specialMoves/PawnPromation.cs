using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Diagnostics;

namespace ChessGame.ChessGameMoves.SpecialMoves
{
    /// <summary>
    /// Class handles the pawn promotion logic in a chess game.
    /// It checks if a pawn has reached the last rank and allows the player to select a piece for promotion.
    /// </summary>
    public class PawnPromotion : ObservableObject
    {
        #region -- Constructor -- public PawnPromotion() { }
        public PawnPromotion()
        {
        }
        #endregion

        #region -- Method --

        /// <summary>
        /// Method checks if a pawn is promoted based on its current location
        /// </summary>
        /// <param name="chessPiece"> chess piece </param>
        /// <param name="currentLocation"> current location </param>
        /// <returns> true if promoted, false otherwise </returns>
        public static bool IsPawnPromoted(ChessPiece chessPiece, ChessSquareLocation currentLocation)
        {
            if (chessPiece is Pawn pawn)
            {
                // Check if the pawn is on the last rank for promotion
                if (pawn.CurrentLocation.X == 0 || pawn.CurrentLocation.X == 7)
                {
                    return true;
                }
            }  
            return false;
        }

        #endregion

        #region -- Chess Selection Method --

        /// <summary>
        /// Method returns the selected chess piece based on the piece type
        /// </summary>
        /// <param name="selectedChessPiece"> selected chess piece </param>
        /// <returns> ChessPiece </returns>
        public static ChessPiece? GetSelectedChessPiece(ChessPiece selectedChessPiece)
        {
            return selectedChessPiece.PieceType switch
            {
                PieceType.BISHOP => new Bishop(
                                       selectedChessPiece.ChessGameService,
                                       PieceType.BISHOP,
                                       selectedChessPiece.PieceColor,
                                       selectedChessPiece.CurrentLocation,
                                       selectedChessPiece.PieceImage),
                PieceType.KNIGHT => new Knight(
                                       selectedChessPiece.ChessGameService,
                                       PieceType.KNIGHT,
                                       selectedChessPiece.PieceColor,
                                       selectedChessPiece.CurrentLocation,
                                       selectedChessPiece.PieceImage),
                PieceType.ROOK => new Rook(
                                       selectedChessPiece.ChessGameService,
                                       PieceType.ROOK,
                                       selectedChessPiece.PieceColor,
                                       Special_Moves.CASLTING,
                                       selectedChessPiece.CurrentLocation,
                                       selectedChessPiece.PieceImage,
                                        CastlingSide.NOCASTERSIDE),
                PieceType.QUEEN => new Queen(
                                       selectedChessPiece.ChessGameService,
                                       PieceType.QUEEN,
                                       selectedChessPiece.PieceColor,
                                       selectedChessPiece.CurrentLocation,
                                       selectedChessPiece.PieceImage),
                _ => null,
            };
        }

        #endregion
    }
}
