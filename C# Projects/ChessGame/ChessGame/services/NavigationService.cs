using ChessGame.Utilities;
using ChessGame.Interfaces.Services;

namespace ChessGame.Services
{
    /// <summary>
    /// NavigationService class is responsible for managing the navigation between different views in the application. 
    /// It implements the INavigationService interface and provides functionality to navigate to a specific view model type. 
    /// The class uses a factory function to create instances of view models and maintains the current view model being displayed.
    /// </summary>
    class NavigationService : ObservableObject, INavigationService
    {
        /// <summary>
        /// Factory function to create instances of view models based on their type.
        /// </summary>
        private readonly Func<Type,Utilities.ViewModel> _viewModelFactory;
        private Utilities.ViewModel? _currentView = default!;

        /// <summary>
        /// Gets the current view model being displayed. This property is updated whenever navigation occurs to a different view model.
        /// </summary>
        public Utilities.ViewModel? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        /// <summary>
        /// Initializes a new instance of the NavigationService class with the specified view model factory function.
        /// </summary>
        /// <param name="viewModelFactory">A factory function to create instances of view models based on their type.</param>
        public NavigationService(Func<Type, Utilities.ViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Navigates to the specified view model type.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model to navigate to.</typeparam>
        public void NavigateTo<TViewModel>() where TViewModel : Utilities.ViewModel
        {
            Utilities.ViewModel viewModel = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentView = viewModel;
        }
    }
}
