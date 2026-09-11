using ChessGame.Utilities;
using ChessGame.Interfaces.Services;

namespace ChessGame.Services
{
    /// <summary>
    /// Represents a service that provides access to the main view model of the application. 
    /// This service implements the IIObjectService interface and is responsible for managing the main view model instance.
    /// </summary>
    public class IObjectService : IIObjectService
    {
        /// <summary>
        /// Gets or sets the main view model instance. This property is used to access the main view model throughout the application.
        /// </summary>
        public MainViewModelBase mainViewModel { get;  set; }

        /// <summary>
        /// Initializes a new instance of the IObjectService class, setting up the main view model instance.
        /// </summary>
        public IObjectService()
        {
            mainViewModel = new MainViewModelBase();
        }
    }
}
