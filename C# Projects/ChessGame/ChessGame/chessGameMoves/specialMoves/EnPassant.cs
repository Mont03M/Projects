using ChessGame.Square;
using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Collections.ObjectModel;

namespace ChessGame.ChessGameMoves.SpecialMoves
{
    /// <summary>
    /// Class handles the enPassant special move for pawns
    /// </summary>
    public class EnPassant : ObservableObject
    {
        #region -- Properties --
        private bool EnPassantMoveEnabled_ { get; set; } = false;
        private int EnPassantMoveSetCount_ { get; set; } = -1;
        private ChessSquareLocation EnPassentAttackSquare_ { get; set; }

        public bool EnPassantMoveEnabled
        {
            get => EnPassantMoveEnabled_;
            set
            {
                EnPassantMoveEnabled_ = value;
                OnPropertyChanged(nameof(EnPassantMoveEnabled));
            }
        }

        public int EnPassantMoveSetCount
        {
            get => EnPassantMoveSetCount_;
            set
            {
                EnPassantMoveSetCount_ = value;
                OnPropertyChanged(nameof(EnPassantMoveSetCount_));
            }
        }

        public ChessSquareLocation EnPassentAttackSquare
        {
            get => EnPassentAttackSquare_;
            set
            {
                EnPassentAttackSquare_ = value;
                OnPropertyChanged(nameof(EnPassentAttackSquare));
            }

        }
        private List<(ChessPieceColors chessPieceColor, int PawnIndex)> IsPawnMoved { get; set; }

        #endregion

        #region -- Constructor --
        public EnPassant()
        {
            IsPawnMoved = [];
        }

        #endregion

        #region -- Methods --

        /// <summary>
        /// Method checks if a pawn has made its first move and it was two squares
        /// </summary>
        /// <param name="pawn"> chess piece of type pawn </param>
        /// <param name="startLocation"> The starting location of the pawn </param>
        /// <param name="location"> The current location of the pawn </param>
        /// <param name="moved"> Indicates if the pawn has moved </param>
        /// <returns> True if the pawn made its first move and it was two squares, false otherwise </returns>
        public bool IsFirstMoveEnPassant(Pawn pawn, ChessSquareLocation startLocation, ChessSquareLocation location, bool moved)
        {
            // check if pawn has moved and it was not a two square move
            if (moved && Math.Abs(startLocation.X - location.X) != 2 
                && !IsPawnMoved.Any(p => p.chessPieceColor == pawn.PieceColor && p.PawnIndex == pawn.PawnIndex))
            {
                // add pawn to list of pawns that have moved
                lock (SyncRoot.MovesLock)
                {
                    IsPawnMoved.Add((pawn.PieceColor, pawn.PawnIndex));
                }

                return false;
            }
            // check if pawn has moved and it was a two square move
            else if (moved && Math.Abs(startLocation.X - location.X) == 2 
                && !IsPawnMoved.Any(p => p.chessPieceColor == pawn.PieceColor && p.PawnIndex == pawn.PawnIndex))
            {
                // add pawn to list of pawns that have moved
                lock (SyncRoot.MovesLock)
                {
                    IsPawnMoved.Add((pawn.PieceColor, pawn.PawnIndex));
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Method takes a pawn that maded its first move and it was two squares
        /// check if any opponents pawn pieces lie next to the pawn
        /// if a pawn is found next to a pawn that has moved two squares
        /// add enPassant attack squares to its special moves list
        /// </summary>
        /// <param name="chessPiece"> ChessPiece </param>
        /// <param name="chessBoard"> ObservableCollection<ChessSquare> </param>
        /// <param name="MoveCount"> int </param>
        /// <returns> tuple (bool, ChessPieceColors, string) </returns>
        public (bool IsAddEnPassantAttack, ChessPieceColors? IsAddToPlayer, string? ChessBoardFileRank, ChessPiece? Pawn) 
            AddEnPassentSquareToOpponent(ChessPiece chessPiece, ObservableCollection<ChessSquare> chessBoard)
        {
            ChessSquare? left = null;
            ChessSquare? right = null;

            // check if piece is pawn
            if (chessPiece is Pawn pawn)
            {
                // calculate attack square
                this.EnPassentAttackSquare = CheckPawnLocation(pawn);

                // get square 
                var attackSqaure = this.EnPassentAttackSquare.GetSquare(chessBoard);

                // pawn is not in file 1
                if (pawn.CurrentLocation.Y != 0)
                {
                    // check left square for opponent pawn
                    left = new ChessSquareLocation(
                        pawn.CurrentLocation.X,
                        pawn.CurrentLocation.Y - 1
                        ).GetSquare(chessBoard);
                }

                // pawn is not in file 7
                if (pawn.CurrentLocation.Y != 7)
                {
                    // check right square for opponent pawn
                    right = new ChessSquareLocation(
                        pawn.CurrentLocation.X, 
                        pawn.CurrentLocation.Y + 1
                        ).GetSquare(chessBoard);
                }

                // check if there is pawn on either side of this pawn
                if (left != null &&
                    right != null &&
                    left.ChessPiece_ is Pawn pawnLeft &&
                    right.ChessPiece_ is Pawn pawnRight &&
                    pawnLeft.SpecialMoves != null &&
                    pawnRight.SpecialMoves != null &&
                    pawnLeft.PieceColor != pawn.PieceColor &&
                    pawnRight.PieceColor != pawn.PieceColor)
                {
                    // set values and mutate special moves under shared lock
                    lock (SyncRoot.MovesLock)
                    {
                        pawnLeft.CanCaptureEnpassant = true;
                        pawnRight.CanCaptureEnpassant = true;

                        this.EnPassantMoveEnabled = true;

                        // add enPassant square to pawn opponent on left
                        pawnLeft.SpecialMoves.Add(
                            new MovesAvailable(
                            MoveType.EnPASSENT,
                            pawnLeft.PieceType,
                            EnPassentAttackSquare,
                            null!,
                            pawn));

                        // add enPassant square to pawn oppnent on right
                        pawnRight.SpecialMoves.Add(
                            new MovesAvailable(
                            MoveType.EnPASSENT, 
                            pawnRight.PieceType, 
                            EnPassentAttackSquare,
                            null!,
                            pawn));
                    }

                    return (true, pawnLeft.PieceColor, attackSqaure?.ChessBoardLocation, pawn);

                }
                // check left side of pawn -- opponent on left
                else if (left != null &&
                    left.ChessPiece_ is Pawn pawnLeft_ &&
                    pawnLeft_.SpecialMoves != null &&
                    pawnLeft_.PieceColor != pawn.PieceColor)
                {
                    // pawn on left side add attack square
                    pawnLeft_.CanCaptureEnpassant = true;

                    this.EnPassantMoveEnabled = true;

                    pawnLeft_.SpecialMoves.Add(
                        new MovesAvailable(
                        MoveType.EnPASSENT,
                        pawnLeft_.PieceType,
                        EnPassentAttackSquare,
                        null!,
                        pawn));

                    return (true, pawnLeft_.PieceColor, attackSqaure?.ChessBoardLocation, pawn);
                }
                // check right -- opponet on right side
                else if (right != null &&
                    right.ChessPiece_ is Pawn pawnRight_ &&
                    pawnRight_.SpecialMoves != null &&
                    right.ChessPiece_.PieceColor != pawn.PieceColor)
                {
                    pawnRight_.CanCaptureEnpassant = true;

                    this.EnPassantMoveEnabled = true;

                    // add enPassant attack square
                    pawnRight_.SpecialMoves.Add(
                        new MovesAvailable(
                        MoveType.EnPASSENT,
                        pawnRight_.PieceType,
                        EnPassentAttackSquare,
                        null!,
                        pawn));

                    return (true, pawnRight_.PieceColor, attackSqaure?.ChessBoardLocation, pawn);
                }
            }

            // no opponent pawns found on either side of pawn that moved two squares
            return (false, null, string.Empty, null);
        }

        #endregion

        #region -- Helper Method --

        /// <summary>
        /// Helper Method gets the pawn location of the current pawn
        /// pawn in rank 4 -- moving upward
        /// pawn in rank 5 -- moving downward
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static ChessSquareLocation CheckPawnLocation(Pawn pawn)
        {
            // bottom to top -- player chess pieces
            if(pawn.CurrentLocation.X == 4)
            {
                return new ChessSquareLocation(pawn.CurrentLocation.X+1, pawn.CurrentLocation.Y);
            }
            // top to bottom -- opponet chess pieces
            else
            {
                return new ChessSquareLocation(pawn.CurrentLocation.X-1, pawn.CurrentLocation.Y);
            }
        }

        #endregion
    }
}
