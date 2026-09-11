using ChessGame.ViewModels;
using ChessGame.Interfaces.Services;

namespace ChessGame.Utilities
{
    /// <summary>
    /// Represents the base class for the main view model in the chess game application.
    /// </summary>
    public class MainViewModelBase: ViewModel
    {
        // navigation service
        private INavigationService? _navigation;

        // Gets or sets the navigation service used for navigating between views.
        public INavigationService? Navigation
        {
            get => _navigation;
            set
            {
                _navigation = value;
                OnPropertyChanged(nameof(Navigation));
            }
        }

        // Commands for navigation and actions
        public RelayCommand? NavigateHomeUICommand { get; set; }
        public RelayCommand? NavigateDevOptionsCommand { get; set; }
        public RelayCommand? NavigateGameOptionsCommand { get; set; }
        public RelayCommand? NavigateHomeUICancelCommand { get; set; }
        public RelayCommand? NavigateToGameBoardCommand {  get; set; }
        public RelayCommand? NavigateGameRulesCommand {  get; set; }
        public RelayCommand? NavigateSettingsCommand { get; set; }
        public RelayCommand? ExecuteGenerateAllAvailableMovesCommand { get; set; }

        // Async commands for navigation and actions
        public AsyncRelayCommand? NavigateHomeUICancelCommand_ { get; set; }
        public AsyncRelayCommand? NavigateGameOptionsCommand_ { get; set; }
        public AsyncRelayCommand? NavigateHomeUICommand_ { get; set; }
        public AsyncRelayCommand? NavigateHomeFromBoardCommand { get; set; }
        public AsyncRelayCommand? ResignCommand { get; set; }
        public AsyncRelayCommand? ExecuteMovePieceCommand { get; set; }
        public AsyncRelayCommand? ExecutePawnPromotionCommand {get; set;}
        public AsyncRelayCommand? PauseGameCommand { get; set; }
        public AsyncRelayCommand? ResetSettingServiceColors { get; set; }
        public AsyncRelayCommand? ResetToDefaultColors { get; set; }

        public MainViewModelBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModelBase"/> class with the specified navigation service and main view model object.
        /// </summary>
        /// <param name="navService">The navigation service used for navigating between views.</param>
        /// <param name="mainViewModelObject">The main view model object.</param>
        public MainViewModelBase(INavigationService navService, IIObjectService mainViewModelObject)
        {
            // create object
            mainViewModelObject.mainViewModel = this;

            // set current value
            Navigation = navService;

            // set pre-defined commands
            NavigateSettingsCommand = new RelayCommand(o => { Navigation.NavigateTo<SettingsVM>();}, o => true);
            NavigateHomeUICommand = new RelayCommand(o => { Navigation.NavigateTo<MainChessGameUI_VM>();}, o => true);
            NavigateHomeUICancelCommand = new RelayCommand(o => { Navigation.NavigateTo<MainChessGameUI_VM>(); }, o => true);
            NavigateGameOptionsCommand = new RelayCommand(o => { Navigation.NavigateTo<GameOptionsVM>(); }, o => true);
            NavigateToGameBoardCommand = new RelayCommand(o => { Navigation.NavigateTo<ChessMatchVM>(); }, o => true);

            // navigate to mainChess UI
            Navigation.NavigateTo<MainChessGameUI_VM>();
        }
    }
}
