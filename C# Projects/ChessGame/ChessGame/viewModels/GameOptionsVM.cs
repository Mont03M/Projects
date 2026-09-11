using ChessGame.Game.chessSoundPlayer;
using ChessGame.Enums;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Utilities;

namespace ChessGame.ViewModels
{
    /// <summary>
    /// Represents the view model for the game options UI.
    /// </summary>
    public class GameOptionsVM : Utilities.ViewModel
    {
        public IIObjectService mainView { get; set; }

        public IIChessGameService chessGameService { get; set; }

        public AppSettingsService appSettingsService { get; set; }

        private string? _selectedLevel;

        private string? _selectedMode;

        private ChessPieceColors? _playerColor;

        private bool _lightingChess { get; set; } = false;

        private bool _lightingChessEnabled { get; set; } = true;

        // selected level
        public string? SelectedLevel
        {
            get => _selectedLevel;
            set
            {
                _selectedLevel = value;
                OnPropertyChanged(nameof(SelectedLevel));
            }
        }

        // selected mode
        public string? SelectedMode
        {
            get => _selectedMode;
            set
            {
                _selectedMode = value;
                OnPropertyChanged(nameof(SelectedMode));

                if (value != null && value.Equals("AI-AI"))
                {
                    IsLightingChess = false;
                    IsLightingChessEnabled = false;
                }
                else
                {
                    IsLightingChessEnabled = true;
                }
            }
        }

        // selected player color
        public ChessPieceColors? PlayerColor
        {
            get => _playerColor;
            set
            {
                _playerColor = value;
                OnPropertyChanged(nameof(PlayerColor));
            }
        }

        // selected timer true/false
        public bool IsLightingChess
        {
            get => _lightingChess;
            set
            {
                _lightingChess = value;
                OnPropertyChanged(nameof(IsLightingChess));
            }
        }

        public bool IsLightingChessEnabled
        {
            get => _lightingChessEnabled;
            set
            {
                _lightingChessEnabled = value;
                OnPropertyChanged(nameof(IsLightingChessEnabled));
            }
        }

        /// <summary>
        /// Initializes a new instance of the GameOptionsVM class.
        /// </summary>
        /// <param name="mainViewModel">The main view model.</param>
        /// <param name="appSettingsService">The application settings service.</param>
        /// <param name="chessGameService">The chess game service.</param>
        public GameOptionsVM(IIObjectService mainViewModel, AppSettingsService appSettingsService, IIChessGameService chessGameService) 
        {
            this.mainView = mainViewModel;
            this.chessGameService = chessGameService;

            this.mainView.mainViewModel.NavigateToGameBoardCommand = new RelayCommand(ExecuteToBoard, CanExecuteToBoard);
            this.mainView.mainViewModel.NavigateHomeUICommand = new RelayCommand(ExecuteToHome);

            this.appSettingsService = appSettingsService;

            if (appSettingsService.Settings.IsEnableSoundEffects)
                ChessSoundPlayer.PlayerNavSound();
        }

        /// <summary>
        /// Executes the navigation to the home view.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        private void ExecuteToHome(object obj)
        {
            Reset();
            this.mainView.mainViewModel.Navigation?.NavigateTo<MainChessGameUI_VM>();
        }

        /// <summary>
        /// Determines whether the navigation to the game board can be executed.
        /// </summary>
        /// <param name="arg">The command parameter.</param>
        /// <returns>True if the navigation can be executed; otherwise, false.</returns>
        private bool CanExecuteToBoard(object arg)
        {
            // check values
            if(!string.IsNullOrWhiteSpace(SelectedLevel) && !string.IsNullOrWhiteSpace(SelectedMode) 
                && (PlayerColor == ChessPieceColors.WHITE || PlayerColor == ChessPieceColors.BLACK))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Executes the navigation to the game board.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        private void ExecuteToBoard(object obj)
        {
            chessGameService.selectedLevel = SelectedLevel;
            chessGameService.selectedGameMode = SelectedMode;
            chessGameService.playerColor = (ChessPieceColors)PlayerColor!;
            chessGameService.opponetColor = GetOpponetColor((ChessPieceColors)PlayerColor!);
            chessGameService.LightingChess = IsLightingChess;
            chessGameService.CurrentTurn = ChessPieceColors.WHITE;
            chessGameService.IsGamePause = false;
            chessGameService.IsGameOver = false;

            Reset();

            // navigate to game board
            this.mainView.mainViewModel.Navigation?.NavigateTo<ChessMatchVM>();
        }

        /// <summary>
        /// Gets the opponent color based on the player's color.
        /// </summary>
        /// <param name="playerColor">The player's color.</param>
        /// <returns>The opponent's color.</returns>
        public ChessPieceColors GetOpponetColor(ChessPieceColors playerColor)
        {
           return (playerColor == ChessPieceColors.WHITE) ? ChessPieceColors.BLACK : ChessPieceColors.WHITE;
        }

        /// <summary>
        /// Resets the game options to their default values.
        /// </summary>
        public void Reset()
        {
            SelectedLevel = new string(" ");
            SelectedMode = new string(" ");
            PlayerColor = null!;
            IsLightingChess = false;
        }
    }
}
