using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Utilities;

namespace ChessGame.ChessGameMoves.SpecialMoves.helpers
{
    /// <summary>
    /// Helper class for managing En Passant move information in a chess game.
    /// </summary>
    public class EnPassantHelper : ObservableObject
    {
        #region -- Properties --
        private string TargetSqure_ { get; set; } = new string("-");
        private int MoveCount_ { get; set; } = 0;
        private ChessPieceColors? ResetColor_ { get; set; }
        private ChessPieceColors? SetByPlayer_ { get; set; }
        private ChessPieceColors? SetForPlayer_ { get; set; }
        private ChessPiece? Pawn_ { get; set; }
        public string TargetSquare
        {
            get => TargetSqure_;
            set
            {
                TargetSqure_ = value;
                OnPropertyChanged(nameof(TargetSquare));
            }
        }

        public int MoveCount
        {
            get => MoveCount_;
            set
            {
                MoveCount_ = value;
                OnPropertyChanged(nameof(MoveCount));
            }
        }

        public ChessPieceColors? ResetColor
        {
            get => ResetColor_;
            set
            {
                ResetColor_ = value;
                OnPropertyChanged(nameof(ResetColor));
            }
        }

        public ChessPieceColors? SetByPlayer
        {
            get => SetByPlayer_;
            set
            {
                SetForPlayer_ = value;
                OnPropertyChanged(nameof(SetByPlayer));
            }
        }

        public ChessPieceColors? SetForPlayer
        {
            get => SetForPlayer_;
            set
            {
                SetForPlayer_ = value;
                OnPropertyChanged(nameof(SetForPlayer));
            }
        }

        public ChessPiece? Pawn
        {
            get => Pawn_;
            set
            {
                Pawn_ = value;
                OnPropertyChanged(nameof(Pawn));
            }
        }
        #endregion

        #region -- Constructor --

        /// <summary>
        /// Constructor for EnPassantHelper class
        /// </summary>
        /// <param name="targetSquare">The square to which the pawn can perform en passant capture.</param>
        /// <param name="moveCount">The number of moves made by the pawn.</param>
        /// <param name="resetColor">The color of the pawn that can be reset.</param>
        /// <param name="setByPlayer">The color of the player who set the pawn.</param>
        /// <param name="setForPlayer">The color of the player for whom the pawn is set.</param>
        /// <param name="pawn">The pawn involved in the en passant move.</param>
        public EnPassantHelper(string targetSquare, int moveCount, ChessPieceColors? resetColor, ChessPieceColors? setByPlayer, 
            ChessPieceColors? setForPlayer, ChessPiece? pawn)
        {
            TargetSquare = targetSquare;
            MoveCount = moveCount;
            ResetColor = resetColor;
            SetByPlayer = setByPlayer;
            SetForPlayer = setForPlayer;
            Pawn = pawn;
        }

        #endregion
    }
}
