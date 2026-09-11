namespace ChessGame.Interfaces.Services
{
    /// <summary>
    /// Defines a service for navigating between different views in the application. It provides methods to navigate to a specific view model and retrieve the current view model.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the current view model that is being displayed in the application.
        /// </summary>
        Utilities.ViewModel? CurrentView { get; }

        /// <summary>
        /// Navigates to the specified view model type. This method allows the application to switch to a different view by providing the type of the view model to navigate to.
        /// </summary>
        /// <typeparam name="T">The type of the view model to navigate to.</typeparam>
        void NavigateTo<T>() where T : Utilities.ViewModel;
    }
}
