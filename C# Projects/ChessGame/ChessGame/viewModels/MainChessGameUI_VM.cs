using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Utilities;

namespace ChessGame.ViewModels
{
    /// <summary>
    /// Represents the view model for the main chess game UI.
    /// </summary>
    class MainChessGameUI_VM : Utilities.ViewModel
    {
        public IIObjectService MainView { get; set; }

        private AppSettingsService AppSettingsService { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainChessGameUI_VM class.
        /// </summary>
        /// <param name="mainViewModel">The main view model.</param>
        /// <param name="appSettingsService">The application settings service.</param>
        public MainChessGameUI_VM(IIObjectService mainViewModel, AppSettingsService appSettingsService) 
        {
            AppSettingsService = appSettingsService;

            this.MainView = mainViewModel;

            // Navigate to GameOptionsVM command
            this.MainView.mainViewModel.NavigateGameOptionsCommand = new RelayCommand(
                o => { this.MainView?.mainViewModel?.Navigation?.NavigateTo<GameOptionsVM>(); }, o => true
                );
        }
    }
}
