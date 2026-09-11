using ChessGame.ChessBoard;
using ChessGame.ChessPieces;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Enums;
using ChessGame.Square;
using ChessGame.Structs;
using System.Collections.ObjectModel;

namespace ChessGame.ChessGameMoves.SpecialMoves
{
    /// <summary>
    /// The Castling class provides methods to determine if a player can perform a castling move in chess, based on the current state of the chess board, 
    /// the positions of the rook and king, and the squares attacked by the opponent. It includes methods to check if the move is valid, 
    /// generate the path for the castling move, and determine the new locations for the rook and king after castling.
    /// </summary>
    public class Castling
    {

        #region -- Driver Method ---

        /// <summary>
        /// Driver Method checks if a player can perform a caslting move based on the current state of the chess board
        /// </summary>
        /// <param name="r">The rook involved in the castling move.</param>
        /// <param name="k">The king involved in the castling move.</param>
        /// <param name="SquaresAttacked">The list of squares attacked by the opponent.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>True if the player can perform a castling move, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsCaslting(Rook r, King k, 
            List<(ChessSquareLocation AttackedSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked, Board chessBoard)
        {
            // check for null arguments
            if (r is null || k is null) throw new ArgumentNullException(nameof(r));

            ArgumentNullException.ThrowIfNull(k);

            // check if the move is a valid castling move
            if (CheckIsCasltingMove((Rook?)r, (King?)k, 
                    k.PieceColor, SquaresAttacked, chessBoard))
            {
               return true;
            }
            
            return false;
        }

        #endregion

        #region -- Helper Methods ---

        /// <summary>
        /// Checks if the move is a valid castling move based on the current state of the chess board and the positions of the rook and king.
        /// </summary>
        /// <param name="rook_">The rook involved in the castling move.</param>
        /// <param name="king_">The king involved in the castling move.</param>
        /// <param name="pieceColor">The color of the chess pieces.</param>
        /// <param name="SquaresAttacked">The list of squares attacked by the opponent.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>True if the move is a valid castling move, false otherwise.</returns>
        public static bool CheckIsCasltingMove(Rook? rook_, King? king_, ChessPieceColors pieceColor,
          List<(ChessSquareLocation AttackSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked, Board chessBoard)
        {
            if ((rook_ != null && king_ != null))
            {
                // check if the rook and king have not moved yet
                if (rook_.ChessPieceMoved is false && king_.ChessPieceMoved is false)
                {
                    // check if the squares between the rook and king are empty and not attacked by the opponent
                    var opponetChessPieces = ChessPieceExtenstions.GetMoves<ChessSquareLocation>(chessBoard, ChessPieceExtenstions.GetOtherColor(pieceColor), 
                        includePawnPossibleAttackMoves: true);

                    // check if the squares between the rook and king are empty and not attacked by the opponent
                    if (CheckCasltingSquares(rook_, chessBoard)
                         && CheckIsSquareSafe(rook_, SquaresAttacked, opponetChessPieces ?? []))
                    {
                        return true;
                    }
                }
            }
               
            return false;
        }

        /// <summary>
        /// Checks if the squares between the rook and king are safe from opponent attacks.
        /// </summary>
        /// <param name="rook_">The rook involved in the castling move.</param>
        /// <param name="SquaresAttacked">The list of squares attacked by the opponent.</param>
        /// <param name="opponetChessPieces">The list of squares occupied by the opponent's chess pieces.</param>
        /// <returns>True if the squares are safe, false otherwise.</returns>
        private static bool CheckIsSquareSafe(Rook rook_,
            List<(ChessSquareLocation AttackedSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked, List<ChessSquareLocation> opponetChessPieces)
        {
            // check if the squares between the rook and king are safe from opponent attacks
            foreach (var (AttackedSquare, RookChessPieceLoc) in SquaresAttacked)
            {
                // check if the rook's current location is the same as the rook's location in the attacked squares
                if (RookChessPieceLoc == rook_.CurrentLocation)
                {
                    // check if the attacked square is occupied by an opponent's chess piece
                    foreach (var opponetMove in opponetChessPieces)
                    {
                        // check if the attacked square is the same as the opponent's move
                        if (AttackedSquare == opponetMove)
                        {
                            return false;
                        }
                    }

                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the squares between the rook and king are empty on the chess board.
        /// </summary>
        /// <param name="rook_">The rook involved in the castling move.</param>
        /// <param name="chessBoard">The chess board containing the chess squares.</param>
        /// <returns>True if the squares are empty, false otherwise.</returns>
        private static bool CheckCasltingSquares(Rook rook_, 
            Board chessBoard)
        {
            // generate the path for the castling move based on the rook's current location and the castling side
            var moves = CasltingMoves(rook_.CurrentLocation, castlingSide: (CastlingSide)rook_.CastleSide);

            // check if the squares between the rook and king are empty on the chess board
            foreach (var move in moves)
            {
                if(!ChessPiece.IsSquareEmpty(move, 
                    new ObservableCollection<ChessSquare>(chessBoard.ChessSquares_ ?? [])))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates the squares between the rook and king for the castling move based on the rook's current location and the castling side.
        /// </summary>
        /// <param name="initialPoint">The initial location of the rook.</param>
        /// <param name="castlingSide">The side of the castling move.</param>
        /// <returns>A list of chess square locations representing the castling path.</returns>
        private static List<ChessSquareLocation> CasltingMoves(ChessSquareLocation initialPoint, CastlingSide castlingSide)
        {
            List<ChessSquareLocation> casltingMoves = [];

            // white on top screen
            if (initialPoint.Y == 0 && castlingSide == CastlingSide.QUEENSIDE)
            {
                // generate the starting point for the castling move based on the initial point and the castling side
                var startPoint = new ChessSquareLocation(initialPoint.X, 0);

                // generate the squares between the rook and king for the castling move
                for (int i = 1; i <= 3; i++)
                {
                    // generate the next square in the castling path
                    var move = new ChessSquareLocation(startPoint.X, startPoint.Y + i);

                    // check if the generated square is not the same as the initial point
                    if (!move.Equals(initialPoint))
                    {
                        casltingMoves.Add(move);
                    }
                }
            }
            // white on bottom screen
            else if (initialPoint.Y == 0 && castlingSide == CastlingSide.KINGSIDE)
            {
                // generate the starting point for the castling move based on the initial point and the castling side
                var startPoint = new ChessSquareLocation(initialPoint.X, 0);

                for (int i = 1; i < 3; i++)
                {
                    var move = new ChessSquareLocation(startPoint.X, startPoint.Y + i);

                    if (!move.Equals(initialPoint))
                    {
                        casltingMoves.Add(move);
                    }
                }
            }
            // black on top screen
            else if (initialPoint.Y == 7 && castlingSide == CastlingSide.QUEENSIDE)
            {
                var startPoint = new ChessSquareLocation(initialPoint.X, 7);

                for (int i = 1; i <= 3; i++)
                {
                    var move = new ChessSquareLocation(startPoint.X, startPoint.Y - i);

                    if(!move.Equals(initialPoint))
                    {
                        casltingMoves.Add(move);
                    }
                }
            }
            // black on bottom screen
            else if (initialPoint.Y == 7 && castlingSide == CastlingSide.KINGSIDE)
            {
                var startPoint = new ChessSquareLocation(initialPoint.X, 7);

                for (int i = 1; i < 3; i++)
                {
                    var move = new ChessSquareLocation(startPoint.X, startPoint.Y - i);

                    if (!move.Equals(initialPoint))
                    {
                        casltingMoves.Add(move);
                    }
                }
            }

            // return the list of castling moves
            return casltingMoves;
        }

        /// <summary>
        /// Generates the new locations for the rook and king after a castling move based on their current locations and the castling side.
        /// </summary>
        /// <param name="r"> rook chess piece </param>
        /// <param name="k"> king chess piece </param>
        /// <returns> tuple of new locations </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static (ChessSquareLocation rook, ChessSquareLocation king)? CasltingLocations(Rook r, King k)
        {

            if (r is null || k is null) throw new ArgumentNullException(nameof(r));
            ArgumentNullException.ThrowIfNull(k);

            (ChessSquareLocation rook, ChessSquareLocation king) values = (r.CurrentLocation, k.CurrentLocation);

            // white is on top board queen side is on the left side
            if (r.CastleSide is CastlingSide.QUEENSIDE && r.CurrentLocation.Y == 0)
            {
                // generate moves for chess peices on the y=0 axis
                values = (new ChessSquareLocation(r.CurrentLocation.X, r.CurrentLocation.Y + 3),
                    new ChessSquareLocation(k.CurrentLocation.X, k.CurrentLocation.Y - 2));

            }
            // white is on bottom board king side is on the right side
            else if (r.CastleSide is CastlingSide.KINGSIDE && r.CurrentLocation.Y == 7)
            {
                // generate moves for chess peices on the y=7 axis
                values = (new ChessSquareLocation(r.CurrentLocation.X, r.CurrentLocation.Y - 2),
                    new ChessSquareLocation(k.CurrentLocation.X, k.CurrentLocation.Y + 2));
            }
            // black is on top board king side is on the left side
            else if (r.CastleSide is CastlingSide.KINGSIDE && r.CurrentLocation.Y == 0)
            {
                // generate moves for chess peices on the y=0 axis
                values = (new ChessSquareLocation(r.CurrentLocation.X, r.CurrentLocation.Y + 2),
                    new ChessSquareLocation(k.CurrentLocation.X, k.CurrentLocation.Y - 2));
            }
            // black is on the bottom board queen side is on the right side
            else if (r.CastleSide is CastlingSide.QUEENSIDE && r.CurrentLocation.Y == 7)
            {
                // generate moves for chess peices on the y=7 axis
                values = (new ChessSquareLocation(r.CurrentLocation.X, r.CurrentLocation.Y - 3),
                    new ChessSquareLocation(k.CurrentLocation.X, k.CurrentLocation.Y + 2));
            }
          
            return values;
        }

        /// <summary>
        /// determines a chess pieces type for a rook chess piece
        /// </summary>
        /// <param name="piece1"> chess piece</param>
        /// <param name="piece2"> chess piece </param>
        /// <returns> Chess Piece (Rook) </returns>
        public static ChessPiece? CastRook(ChessPiece piece1, ChessPiece piece2)
        {
            return (piece1.PieceType == PieceType.ROOK) ? 
                (Rook)piece1 : (piece2.PieceType == PieceType.ROOK) ? (Rook)piece2 : null;
        }

        /// <summary>
        /// determines a chess pieces type for a king chess piece
        /// </summary>
        /// <param name="piece1"> chess piece</param>
        /// <param name="piece2"> chess piece </param>
        /// <returns> Chess Piece (King) </returns>
        public static ChessPiece? CastKing(ChessPiece piece1, ChessPiece piece2)
        {
            return (piece1.PieceType == PieceType.KING) ? 
                (King)piece1 : (piece2.PieceType == PieceType.KING) ? (King)piece2 : null;
        }
    }

        #endregion
}