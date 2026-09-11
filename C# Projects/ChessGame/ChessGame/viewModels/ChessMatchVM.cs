using ChessGame.Game.chessSoundPlayer;
using ChessGame.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Utilities;
using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ChessGame.ViewModels
{
    /// <summary>
    /// Represents the view model for a chess match, managing game state, timers, and user interactions.
    /// </summary>
    public class ChessMatchVM : Utilities.ViewModel
    {
        // Services
        public IIObjectService MainView { get; set; }
        private IIChessGameService ChessGameService { get; set; }
        public AppSettingsService AppSettingsService { get; set; }

        // timer variables
        private DispatcherTimer gameTimer = null!;
        private TimeSpan gameTimeRemaining;
        private TimeSpan gameCountdownDuration = TimeSpan.FromSeconds(1800);
        private DispatcherTimer playerTimer = null!;
        private TimeSpan playerTimeRemaining;
        private readonly TimeSpan playerCountDownDuration = TimeSpan.FromSeconds(31);

        // UI elements binding
        private string? gameClock;
        private string? playerMoveClock;
        private Visibility? clockVisibility { get; set; } = Visibility.Hidden;
        private string? clockButtonContent;
        private string? buttonCommandParam;
        // Property values

        // game clock 
        public string? GameClock_
        {
            get => gameClock;
            set
            {
                gameClock = value;
                OnPropertyChanged(nameof(GameClock_));
            }
        }

        // player clock
        public string? PlayerMoveClock
        {
            get => playerMoveClock;
            set
            {
                playerMoveClock = value;
                OnPropertyChanged(nameof(PlayerMoveClock));
            }
        }

        // clock visible
        public Visibility? ClockVisibility
        {
            get => clockVisibility;
            set
            {
                clockVisibility = value;
                OnPropertyChanged(nameof(ClockVisibility));
            }
        }

        // content of button
        public string? ClockButtonContent
        {
            get => clockButtonContent;
            set
            {
                clockButtonContent = value;
                OnPropertyChanged(nameof(ClockButtonContent));
            }
        }

        // button param
        public string? ButtonCommandParam
        {
            get => buttonCommandParam;
            set
            {
                buttonCommandParam = value;
                OnPropertyChanged(nameof(ButtonCommandParam));
            }
        }

        // chess game
        public Game.ChessGame ChessGame { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChessMatchVM"/> class with the specified services.
        /// </summary>
        /// <param name="mainViewModel">The main view model service.</param>
        /// <param name="chessGameService">The chess game service.</param>
        /// <param name="appSettingsService">The application settings service.</param>
        public ChessMatchVM(IIObjectService mainViewModel, IIChessGameService chessGameService, 
            AppSettingsService appSettingsService) 
        {
           
            // services
            MainView = mainViewModel;
            this.ChessGameService = chessGameService;
            this.AppSettingsService = appSettingsService;

            // set IsEndOfGame delegate to chessGameService
            chessGameService.IsEndOfGame = IsEndOfGame;

            // start timers if enabled
            StartTimers();

            // initialize chess game
            ChessGame = new Game.ChessGame(mainViewModel, chessGameService, appSettingsService);

            // set commands
            MainView.mainViewModel.NavigateGameOptionsCommand_ = new AsyncRelayCommand(ExecuteBackToGameOptions);
            MainView.mainViewModel.ResignCommand = new AsyncRelayCommand(ExecuteResignCommand);
            MainView.mainViewModel.NavigateHomeFromBoardCommand = new AsyncRelayCommand(ExecuteToHomeResetCommand);
            MainView.mainViewModel.PauseGameCommand = new AsyncRelayCommand(ExecutePauseGameCommand);

            // play sound effect if enabled
            if (appSettingsService.Settings.IsEnableSoundEffects)
                ChessSoundPlayer.PlayMatchStart();

        }

        /// <summary>
        /// Executes the command to navigate back to the game options, prompting the user for confirmation and handling game state accordingly.
        /// </summary>
        /// <param name="arg">The command parameter.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecuteBackToGameOptions(object? arg)
        {
            ChessGameService.IsGamePause = true;

            if (ChessGameService.LightingChess)
            {
                StopTimers();
            }

            var panel = CustomDialogMessage.CustomDialogPanel("Leave Match", "Are you sure you want to leave the current game?\nYour progress will be lost.", transparency: true);
            var result = await DialogHost.Show(panel, "RootDialog");

            if((bool?)result == true)
            {
                await ChessGame.ExitGame();


                this.MainView?.mainViewModel?.Navigation?.NavigateTo<GameOptionsVM>();
            }
            else
            {
                IsInPauseMode();
                await ChessGame.RestartGameLoop((ChessGameService.selectedGameMode != null && ChessGameService.selectedGameMode.Equals("AI-AI")) ? true : false);
            }
        }

        /// <summary>
        /// Executes the command to resign the current match, prompting the user for confirmation and handling game state accordingly.
        /// </summary>
        /// <param name="arg">The command parameter.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecuteResignCommand(object? arg)
        {
            ChessGameService.IsGamePause = true;

            if (ChessGameService.LightingChess)
                StopTimers();

            var panel = CustomDialogMessage.CustomDialogPanel("Resign Match?", "Are you sure you want to resign the current match?\nYour progress will be lost.",
                 restartButtonMsg: "New Match", transparency: true);
            var result = await DialogHost.Show(panel, "RootDialog");


            if ((bool?)result == true)
            {
                await ResetGame(result);

                if (AppSettingsService.Settings.IsEnableSoundEffects)
                    ChessSoundPlayer.PlayMatchStart();
            }
            else
            {
                IsInPauseMode();
                await ChessGame.RestartGameLoop((ChessGameService.selectedGameMode != null && ChessGameService.selectedGameMode.Equals("AI-AI")) ? true : false);
            }
        }

        /// <summary>
        /// Executes the command to pause or resume the game based on the provided parameter, updating the game state and timers accordingly.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecutePauseGameCommand(object? obj)
        {
            // convert value
            string? IsGamePause = obj as string;

            // check value
            if (!string.IsNullOrWhiteSpace(IsGamePause) && bool.Parse(IsGamePause)){

                // pause chessboard
                ChessGameService.IsGamePause = true;

                // stop timers
                StopTimers();

                ClockButtonContent = "Resume Game";
                ButtonCommandParam = "false";
            }
            else
            {
                
                ClockButtonContent = "Pause Game";
                ButtonCommandParam = "true";

                // wait one seconds after un-pausing 
                await Task.Delay(1000);

                // resume timers
                StartUpTimers();

                // un-pause game
                await ChessGame.UnPauseGame();
            }
        }

        /// <summary>
        /// Executes the command to navigate to the home screen, prompting the user for confirmation and handling game state accordingly.
        /// </summary>
        /// <param name="arg">The command parameter.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ExecuteToHomeResetCommand(object? arg)
        {
            ChessGameService.IsGamePause = true;

            if (ChessGameService.LightingChess)
            {
                StopTimers();
            }

            var panel = CustomDialogMessage.CustomDialogPanel("Navigate Home", "Are you sure you want to leave the current game?\nYour progress will be lost.", transparency: true);
            var result = await DialogHost.Show(panel, "RootDialog");

            if ((bool?)result == true)
            {
                await ChessGame.ExitGame();

                this.MainView?.mainViewModel?.Navigation?.NavigateTo<MainChessGameUI_VM>();
            }
            else
            {
                IsInPauseMode();
                await ChessGame.RestartGameLoop((ChessGameService.selectedGameMode != null && ChessGameService.selectedGameMode.Equals("AI-AI")) ? true : false);
            }
        }

        /// <summary>
        /// Handles the end of the game by displaying a dialog with options to restart or exit, and performs actions based on the user's choice.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="contentMessage">The content message of the dialog.</param>
        /// <param name="cancelButtonMsg">The text for the cancel button.</param>
        /// <param name="restartButtonMsg">The text for the restart button.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task IsEndOfGame(string title, string contentMessage, string cancelButtonMsg, string restartButtonMsg)
        {
            Debug.WriteLine("Invoked in IsEndOfGame!!!!");

            if (ChessGameService.LightingChess)
                StopTimers();

            if(AppSettingsService.Settings.IsEnableSoundEffects)
               ChessSoundPlayer.PlayCheckmate();

            var panel = CustomDialogMessage.CustomDialogPanel(title, contentMessage, cancelButtonMsg, restartButtonMsg, transparency: true);
            var result = await DialogHost.Show(panel, "RootDialog");

            if((bool?) result == true)
            {
                await ResetGame(result);
            }
            else
            {
                await ChessGame.ExitGame();

                this.MainView?.mainViewModel.Navigation?.NavigateTo<MainChessGameUI_VM>();
            }
        }


        /// <summary>
        /// Starts the game clock timer, initializing the remaining time and setting up the timer tick event to update the game clock display and handle game over conditions.
        /// </summary>
        private void StartGameClock()
        {
            // if another gameTimer timer is set
            // stop timer
            if (gameTimer != null)
            {
                gameTimer?.Stop();
            }

            // set timer timer
            gameTimeRemaining = gameCountdownDuration;
            gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            gameTimer.Tick += new EventHandler(TimerTick);
            gameTimer.Start();
        }

        /// <summary>
        /// Handles the tick event of the game clock timer, updating the remaining time, checking for game over conditions, and updating the game clock display accordingly.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        void TimerTick(object? sender, EventArgs e)
        {
            // subtract 1 sec from remaining time
            gameTimeRemaining -= TimeSpan.FromMilliseconds(500);

            // check if game is over
            if(gameTimeRemaining <= TimeSpan.Zero)
            {
                GameClock_ = "00:00:00";
                gameTimer.Stop();

                ChessGameService.GameTimeRemaining -= TimeSpan.FromMilliseconds(500);
                ChessGameService.GameTimeRemaining = gameTimeRemaining;
            }
            else
            {
                // show timer
                GameClock_ = gameTimeRemaining.ToString(@"hh\:mm\:ss");
                ChessGameService.GameTimeRemaining = gameTimeRemaining;
            }
        }

        /// <summary>
        /// Starts the player clock timer, initializing the remaining time and setting up the timer tick event to update 
        /// the player clock display and handle time expiration conditions.
        /// </summary>
        private void PlayerClock()
        {
            // check if timer is active
            // stop if active
            if (playerTimer != null)
            {
                playerTimer?.Stop();
            }

            // set timer
            playerTimeRemaining = playerCountDownDuration;
            playerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            playerTimer.Tick += new EventHandler(TimerTick_);
            playerTimer.Start();
        }

        // timer for player
        void TimerTick_(object? sender, EventArgs e)
        {
            // subtract 1 sec from time remaining
            playerTimeRemaining -= TimeSpan.FromMilliseconds(500);
            
            // check remaining time
            if (playerTimeRemaining <= TimeSpan.Zero)
            {
                PlayerMoveClock = "00:00:00";
                playerTimer?.Stop();


                playerTimeRemaining -= TimeSpan.FromMilliseconds(500);
                ChessGameService.PlayerTimeRemaining = playerTimeRemaining;
            }
            else
            {
                // show player remaining time
                PlayerMoveClock = playerTimeRemaining.ToString(@"hh\:mm\:ss");
                ChessGameService.PlayerTimeRemaining = playerTimeRemaining;
            }
        }

        /// <summary>
        /// Starts both the game and player timers if the LightingChess setting is enabled.
        /// </summary>
        public void StartTimers()
        {
            
            // if gameSettings for timer are enabled start timers
            if (ChessGameService.LightingChess)
            {
                ClockVisibility = Visibility.Visible;

                Application.Current.Dispatcher.Invoke(async () =>
                {
                    StartGameClock();
                    PlayerClock();
                });
              
                ClockButtonContent = "Pause Game";
                ButtonCommandParam = "true";

                ChessGameService.PlayerClockReset = PlayerClock;
                ChessGameService.GameClockReset = StartGameClock;
                ChessGameService.StopPlayerTimer = StopPlayerTimer;
                ChessGameService.PlayerTimeRemaining = playerTimeRemaining;
                ChessGameService.GameTimeRemaining = gameTimeRemaining;
            }
            else
            {
                ClockVisibility = Visibility.Hidden;
            }
        }
        
        /// <summary>
        /// Resets the game if the result is true, otherwise resumes the game loop.
        /// </summary>
        /// <param name="result">The result indicating whether to reset the game.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>   
        private async Task ResetGame(object? result)
        {
            if ((bool?)result == true)
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await ChessGame.Reset();
                });

                if (ChessGameService.LightingChess)
                {
                    StartTimers();
                }
            }
            else
            {
                IsInPauseMode();
                await ChessGame.RestartGameLoop((ChessGameService.selectedGameMode != null && ChessGameService.selectedGameMode.Equals("AI-AI")) ? true : false);
            }
        }
        
        /// <summary>
        /// Checks if the game is in pause mode and resumes the timers if necessary.
        /// </summary>
        public void IsInPauseMode()
        {
            if (!string.IsNullOrWhiteSpace(ButtonCommandParam) && bool.Parse(ButtonCommandParam) != false)
            {
                ChessGameService.IsGamePause = false;
                StartUpTimers();
            }
        }

        /// <summary>
        /// Starts both the game and player timers, if they are not null, allowing the game to continue from a paused state.
        /// </summary>
        public void StartUpTimers()
        {
            gameTimer?.Start();
            playerTimer?.Start();
        }

        /// <summary>
        /// Stops both the game and player timers, if they are not null, effectively pausing the game and player clocks.
        /// </summary>
        public void StopTimers()
        {
            gameTimer?.Stop();
            playerTimer?.Stop();
        }

        /// <summary>
        /// Stops the player timer, if it is not null, effectively pausing the player clock.
        /// </summary>
        public void StopPlayerTimer()
        {
            playerTimer?.Stop();
        }
    }
}
