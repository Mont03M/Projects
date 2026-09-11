using System.Windows.Input;

namespace ChessGame.Utilities
{
    /// <summary>
    /// Represents a command that can be executed and determines if it can execute.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        private readonly Action<object> _execute;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        private readonly Func<object, bool> _canExecute;

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class with the specified execute action and optional canExecute function.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">A function that determines whether the command can execute.</param>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null!)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter to be passed to the canExecute function.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The parameter to be passed to the execute action.</param>
        public void Execute(object? parameter) => _execute(parameter);
       
    }
}
