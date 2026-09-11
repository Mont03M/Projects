using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChessGame.Utilities
{
    /// <summary>
    /// Represents an observable object that implements the INotifyPropertyChanged interface, allowing property change notifications to be raised.
    /// </summary>
    public class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes, allowing subscribers to be notified of the change.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property name, notifying subscribers that the property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
