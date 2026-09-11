using ChessGame.Utilities;
using System.Collections.ObjectModel;


namespace ChessGame.Game.MoveQueue
{
    /// <summary>
    /// Represents a queue of chess moves, allowing for the storage and management of recent moves in a chess game.
    /// </summary>
    public class MovesQueue: ObservableObject
    {
        private ObservableCollection<ChessMoveInfo> BoardMovesQueue_ { get; set; } = [];

        public ObservableCollection<ChessMoveInfo> BoardMovesQueue
        {
            get => BoardMovesQueue_;

            set
            {
                BoardMovesQueue_ = value;
                OnPropertyChanged(nameof(BoardMovesQueue));
            }
        } 

        private Stack<ChessMoveInfo> MoveStack { get; } = new ();

        public MovesQueue() { }

        /// <summary>
        /// Pushes a new chess move onto the move stack and updates the board moves queue accordingly.
        /// </summary>
        /// <param name="move">The chess move to push onto the stack.</param>
        public void PushMove(ChessMoveInfo move)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                MoveStack.Push(move);

                UpdateBoardMovesStack();
            });
          
        }

        /// <summary>
        /// Updates the board moves queue to reflect the most recent moves in the move stack, keeping only the last 10 moves.
        /// </summary>
        private void UpdateBoardMovesStack()
        {
            BoardMovesQueue.Clear();

            foreach(var move in MoveStack.Take(10))
            {
                BoardMovesQueue.Add(move);
            }
        }

        /// <summary>
        /// Clears the move stack and the board moves queue, effectively resetting the move history.
        /// </summary>
        public void Clear()
        {
            MoveStack.Clear();
            BoardMovesQueue.Clear();
        }

        /// <summary>
        /// Modifies the top move in the move stack using the provided action, and updates the board moves queue accordingly.
        /// </summary>
        /// <param name="modify">The action to apply to the top move in the stack.</param>
        public void ModifyTopOfStack(Action<ChessMoveInfo> modify)
        {
            if (MoveStack.Count == 0)
                return;

            // Use the dispatcher to ensure that the modification and update occur on the UI thread.
            System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {

                modify(MoveStack.Peek());

                UpdateBoardMovesStack();

            });
        }
    }
}
