using ChessGame.Utilities;

namespace ChessGame.Interfaces.Services
{
    /// <summary>
    /// Interface for object services in the chess game application. It defines a contract for classes that provide access to the main view model of the application.
    /// </summary>
    public interface IIObjectService
    {
        /// <summary>
        /// Gets or sets the main view model of the application. This property allows implementing classes to provide access to the main view model, 
        /// which is responsible for managing the application's state and behavior.
        /// </summary>
        MainViewModelBase mainViewModel { get; set; }
    }
}
