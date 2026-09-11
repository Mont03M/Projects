using System.Windows.Input;

namespace ChessGame.Utilities
{
    /// <summary>
    /// A command that allows for asynchronous execution of an action.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        private readonly Func<object?, Task> _execute;
        private readonly Func<object?, bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous action to execute.</param>
        /// <param name="canExecute">A function that determines whether the command can execute.</param>
        public AsyncRelayCommand(Func<object?, Task> execute,
                                 Func<object?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter to evaluate for command execution.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the asynchronous action.</param>
        public async void Execute(object? parameter)   // MUST be void
        {
            if (!CanExecute(parameter))
                return;

            // Mark the command as executing and raise CanExecuteChanged to update the UI
            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute(parameter); // async Task runs here safely
            }
            finally
            {
                // Mark the command as not executing and raise CanExecuteChanged to update the UI
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raises the CanExecuteChanged event to notify the UI that the command's ability to execute has changed.
        /// </summary>
        private void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
