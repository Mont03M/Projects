using ChessGame.Enums;
using ChessGame.Utilities;

namespace ChessGame.Strategies.Tools.StockfishTools
{
    /// <summary>
    /// Represents the move clock for a chess game, tracking the full move count and half move count. 
    /// The full move clock increments after each player's turn, while the half move clock resets to zero after a pawn move or a capture, and increments otherwise. 
    /// This class implements INotifyPropertyChanged to support data binding in WPF applications.
    /// </summary>
    public class StockfishMoveClock : ObservableObject
    {
        /// <summary>
        /// Gets or sets the full move clock count, which increments after each player's turn. The initial value is set to 1.
        /// </summary>
        private int FullMoveClock_ { get; set; } = 1;

        /// <summary>
        /// Gets or sets the half move clock count, which resets to zero after a pawn move or a capture, and increments otherwise. The initial value is set to 0.
        /// </summary>
        private int HalfMoveClock_ { get; set; } = 0;

        public int FullMoveClock
        {
            get => FullMoveClock_;
            set
            {
                FullMoveClock_ = value;
                OnPropertyChanged(nameof(FullMoveClock));
            }
        }

        public int HalfMoveClock
        {
            get => HalfMoveClock_;
            set
            {
                HalfMoveClock_ = value;
                OnPropertyChanged(nameof(HalfMoveClock));
            }
        }

        /// <summary>
        /// Updates the full move clock count based on the provided count. The full move clock is incremented by 1 to reflect the next player's turn.
        /// </summary>
        /// <param name="fullMoveClockCount">The current full move clock count.</param>
        public void UpdateFullMoveClock(int fullMoveClockCount)
        {
            FullMoveClock = (fullMoveClockCount + 1);
        }
        
        /// <summary>
        /// Updates the half move clock count based on the provided move information. The half move clock resets to zero after a pawn move or a capture, and increments otherwise.
        /// </summary>
        /// <param name="moveInfo">The move information for the current move.</param>
        public void UpdateHalfMoveClock(StockChessMoveInfo? moveInfo)
        {
            if ((moveInfo?.ChessMoveType is not null &&
                moveInfo.ChessPieceType is PieceType.PAWN) ||
                (moveInfo?.ChessMoveType is not null &&
                moveInfo.ChessMoveType is MoveType.CAPTURE))

                HalfMoveClock = 0;
            else

                HalfMoveClock += 1;
        }

        /// <summary>
        /// Resets the full move clock to 1 and the half move clock to 0, typically used at the start of a new game or after a reset condition.
        /// </summary>
        public void Reset()
        {
            FullMoveClock = 1;
            HalfMoveClock = 0;
        }

        /// <summary>
        /// Returns a string representation of the current state of the move clocks, including the full move clock and half move clock values.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Full MoveClock: {FullMoveClock} HalfMoveClock {HalfMoveClock}";
        }
    }
}
