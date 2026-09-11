using ChessGame.ChessBoard;
using ChessGame.ChessPieces;
using ChessGame.ChessSets;
using ChessGame.Game.chessSoundPlayer;
using ChessGame.Colors_;
using ChessGame.Controls;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Utilities;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace ChessGame.ViewModels
{
    /// <summary>
    /// Represents the view model for the settings screen of the chess game application.
    /// </summary>
    public class SettingsVM : Utilities.ViewModel
    {
        public IIObjectService MainView { get; set; }

        public AppSettingsService AppSettingsService { get; set; }

        public Board SettingsChessBoard { get; set; }

        public SettingControls SettingsControls { get; set; } = default!;

        public ObservableCollection<ColorItem> MediaColors { get; set; }
        private bool IsCheckSoundEffects { get; set; }
        private string CBSoundEffectTxt { get; set; } = string.Empty;
        private ObservableCollection<string> ChessSetPreviewPieces { get; set; } = default!;

        private Brush oddSquareColor = Brushes.Transparent;

        private Brush evenSquareColor = Brushes.Transparent;

        private Brush availbleMovesIndicatorColor = Brushes.Transparent;

        private Brush attackMovesIndicatorColor = Brushes.Transparent;

        private ColorItem selectedOddSquareColor = default!;

        private ColorItem selectedEvenSquareColor = default!;

        private ColorItem selectedAvailableMovesColor = default!;

        private ColorItem selectedAttackMovesColor = default!;

        private ColorItem checKMateColor = default!;

        private ColorItem specialMoveColor = default!;

        private ColorItem TempSelectedColorHolder = default!;

        private int selectedOddColorIndex = default!;

        private int selectedEvenColorIndex = default!;

        private int selectedAvailableColorIndex = default!;

        private int selectedAttackColorIndex = default!;

        private string selectedChessSet = default!;

        public Brush OddSquareColor
        {
            get => oddSquareColor;
            set
            {
                oddSquareColor = value;
                OnPropertyChanged(nameof(OddSquareColor));
            }
        }

        public Brush EvenSquareColor
        {
            get => evenSquareColor;
            set
            {
                evenSquareColor = value;
                OnPropertyChanged(nameof(EvenSquareColor));
            }
        }

        public Brush AvailbleMovesIndicatorColor
        {
            get => availbleMovesIndicatorColor;
            set
            {
                availbleMovesIndicatorColor = value;
                OnPropertyChanged(nameof(AvailbleMovesIndicatorColor));
            }
        }

        public Brush AttackMovesIndicatorColor
        {
            get => attackMovesIndicatorColor;
            set
            {
                attackMovesIndicatorColor = value;
                OnPropertyChanged(nameof(AttackMovesIndicatorColor));
            }
        }

        public string CheckBoxSoundEffectsTxt
        {
            get => CBSoundEffectTxt;
            set
            {
                CBSoundEffectTxt = value;
                OnPropertyChanged(nameof(CheckBoxSoundEffectsTxt));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether sound effects are enabled in the settings.
        /// </summary>
        public bool IsSoundEffectEnabled
        {
            get => IsCheckSoundEffects;
            set
            {
                IsCheckSoundEffects = value;

                if(IsCheckSoundEffects)
                {
                    Debug.WriteLine("True development enabled");
                    CheckBoxSoundEffectsTxt = new string("Sound Effects Enabled");
                }
                else
                {
                    Debug.WriteLine("False development mode disabled");
                    CheckBoxSoundEffectsTxt = new string("Sound Effects Disabled");
                }

                OnPropertyChanged(nameof(IsSoundEffectEnabled));
            }
        }

        /// <summary>
        /// Gets or sets the selected color for odd squares on the chessboard.
        /// </summary>
        public ColorItem SelectedOddSquareColor
        {
            get => selectedOddSquareColor;
            set
            {
                TempSelectedColorHolder = value;

                // Check if the selected color is not already assigned to other square colors or indicators
                if (!TempSelectedColorHolder.Name.Equals(SelectedEvenSquareColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedAvailableMovesColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedAttackMovesColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(checKMateColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(specialMoveColor.Name))
                {
                    selectedOddSquareColor = value;

                    OddSquareColor = new SolidColorBrush(selectedOddSquareColor.Color);
                    SelectedOddColorIndex = selectedOddSquareColor.Index;

                    SettingsChessBoard.DrawSettingsBoard(SelectedOddSquareColor.Color, SelectedEvenSquareColor.Color);

                    ShowAvailableMoves(selectedAvailableMovesColor, selectedAttackMovesColor);

                    OnPropertyChanged(nameof(SelectedOddSquareColor));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected color for even squares on the chessboard.
        /// </summary>
        public ColorItem SelectedEvenSquareColor
        {
            get => selectedEvenSquareColor;
            set
            {
                TempSelectedColorHolder = value;

                // Check if the selected color is not already assigned to other square colors or indicators
                if (!TempSelectedColorHolder.Name.Equals(SelectedOddSquareColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedAvailableMovesColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedAttackMovesColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(checKMateColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(specialMoveColor.Name))
                {
                    selectedEvenSquareColor = value;

                    EvenSquareColor = new SolidColorBrush(selectedEvenSquareColor.Color);

                    SelectedEvenColorIndex = selectedEvenSquareColor.Index;

                    SettingsChessBoard.DrawSettingsBoard(selectedOddSquareColor.Color, selectedEvenSquareColor.Color);

                    ShowAvailableMoves(selectedAvailableMovesColor, selectedAttackMovesColor);

                    OnPropertyChanged(nameof(SelectedEvenSquareColor));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected color for available moves on the chessboard.
        /// </summary>
        public ColorItem SelectedAvailableMovesColor
        {
            get => selectedAvailableMovesColor;
            set
            {
               
                TempSelectedColorHolder = value;

                // Check if the selected color is not already assigned to other square colors or indicators
                if (!TempSelectedColorHolder.Name.Equals(SelectedAttackMovesColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedOddSquareColor.Name) && 
                    !TempSelectedColorHolder.Name.Equals(SelectedEvenSquareColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(checKMateColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(specialMoveColor.Name))
                {
                    selectedAvailableMovesColor = value;

                    AvailbleMovesIndicatorColor = new SolidColorBrush(selectedAvailableMovesColor.Color);

                    SelectedAvailableColorIndex = selectedAvailableMovesColor.Index;

                    SettingsChessBoard.DrawSettingsBoard(selectedOddSquareColor.Color, selectedEvenSquareColor.Color);

                    ShowAvailableMoves(selectedAvailableMovesColor, selectedAttackMovesColor);

                    OnPropertyChanged(nameof(SelectedAvailableMovesColor));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected color for attack moves on the chessboard.
        /// </summary>
        public ColorItem SelectedAttackMovesColor
        {
            get => selectedAttackMovesColor;
            set
            {
                TempSelectedColorHolder = value;

                // Check if the selected color is not already assigned to other square colors or indicators
                if (!TempSelectedColorHolder.Name.Equals(SelectedAvailableMovesColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedOddSquareColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(SelectedEvenSquareColor.Name) &&
                    !TempSelectedColorHolder.Name.Equals(checKMateColor.Name) && 
                    !TempSelectedColorHolder.Name.Equals(specialMoveColor.Name))
                {
                    selectedAttackMovesColor = value;

                    AttackMovesIndicatorColor = new SolidColorBrush(selectedAttackMovesColor.Color);

                    SelectedAttackColorIndex = selectedAttackMovesColor.Index;

                    SettingsChessBoard.DrawSettingsBoard(selectedOddSquareColor.Color, selectedEvenSquareColor.Color);

                    ShowAvailableMoves(selectedAvailableMovesColor, selectedAttackMovesColor);

                    OnPropertyChanged(nameof(SelectedAttackMovesColor));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected chess set for the game.
        /// </summary>
        public string SelectedChessSet
        {
            get => selectedChessSet;
            set
            {
                selectedChessSet = value;

                if (selectedChessSet.Equals("set1"))
                {
                    ChessPreviewPieces = new ChessSet1().ChessPiecesPreview;
                }
                else if (selectedChessSet.Equals("set2"))
                {
                    ChessPreviewPieces = new ChessSet2().ChessPiecesPreview;
                }
                else if (selectedChessSet.Equals("set3"))
                {
                    ChessPreviewPieces = new ChessSet3().ChessPiecesPreview;
                }

                OnPropertyChanged(nameof(SelectedChessSet));
            }
        }

        /// <summary>
        /// Gets or sets the collection of chess pieces for previewing the selected chess set.
        /// </summary>
        public ObservableCollection<string> ChessPreviewPieces
        {
            get => ChessSetPreviewPieces;
            set
            {
                ChessSetPreviewPieces = value;
                OnPropertyChanged(nameof(ChessPreviewPieces));
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected color for odd squares on the chessboard.
        /// </summary>
        public int SelectedOddColorIndex
        {
            get => selectedOddColorIndex;
            set
            {
                selectedOddColorIndex = value;
                OnPropertyChanged(nameof(SelectedOddColorIndex));
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected color for even squares on the chessboard.
        /// </summary>
        public int SelectedEvenColorIndex
        {
            get => selectedEvenColorIndex;
            set
            {
                selectedEvenColorIndex = value;
                OnPropertyChanged(nameof(SelectedEvenColorIndex));
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected color for available moves on the chessboard.
        /// </summary>
        public int SelectedAvailableColorIndex
        {
            get => selectedAvailableColorIndex;
            set
            {
                selectedAvailableColorIndex = value;
                OnPropertyChanged(nameof(SelectedAvailableColorIndex));
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected color for attack moves on the chessboard.
        /// </summary>
        public int SelectedAttackColorIndex
        {
            get => selectedAttackColorIndex;
            set
            {
                selectedAttackColorIndex = value;
                OnPropertyChanged(nameof(selectedAttackColorIndex));
            }
        }

        /// <summary>
        /// Gets the collection of file labels (A-H) for the chessboard.
        /// </summary>
        public ObservableCollection<string> Files { get; } = [ "A", "B", "C", "D", "E", "F", "G", "H"];

        /// <summary>
        /// Gets the collection of rank numbers (1-8) for the chessboard.
        /// </summary>
        public ObservableCollection<int> Ranks { get; } =
        [
            8, 7, 6, 5, 4, 3, 2, 1
        ];

        /// <summary>
        /// Initializes a new instance of the SettingsVM class with the specified main view model, chess game service, and application settings service.
        /// </summary>
        /// <param name="mainViewModel">The main view model.</param>
        /// <param name="chessGameService">The chess game service.</param>
        /// <param name="appSettingsService">The application settings service.</param>
        public SettingsVM(IIObjectService mainViewModel, IIChessGameService chessGameService, AppSettingsService appSettingsService) 
        {
            // services
            this.MainView = mainViewModel;
            this.AppSettingsService = appSettingsService;

            if (appSettingsService.Settings.IsEnableSoundEffects)
                ChessSoundPlayer.PlayerNavSound();

            // setting selected chess set value
            IsSelectedChessSet();

            IsSetSoundEffect();

            // setting preview board and setting available and attack move indicators 
            SettingsChessBoard = new Board(chessGameService, appSettingsService);
            SettingsChessBoard.DrawSettingsBoard(appSettingsService.Settings?.squareOdd ?? Colors.Transparent,
                appSettingsService.Settings?.squareEven ?? Colors.Transparent);

            if (appSettingsService.Settings != null)
                SettingsControls = new SettingControls(new GameControls(MainView, appSettingsService, SettingsChessBoard, null));

            // setting and loading media colors
            MediaColors = [];
            LoadMediaColors();

            // show available and attack moves
            ShowAvailableMoves(selectedAvailableMovesColor, selectedAttackMovesColor);

            this.MainView.mainViewModel.NavigateHomeUICommand_ = new AsyncRelayCommand(ExecuteSaveChangesThenHome);
            this.MainView.mainViewModel.ResetSettingServiceColors = new AsyncRelayCommand(ExecuteResetSettingServiceColors);
            this.MainView.mainViewModel.ResetToDefaultColors = new AsyncRelayCommand(ExecuteResetToDefault);
            this.MainView.mainViewModel.NavigateHomeUICancelCommand_ = new AsyncRelayCommand(ExecuteCancelReset);
        }

        /// <summary>
        /// Sets the sound effect state based on the application settings and plays a navigation sound if enabled.
        /// </summary>
        private void IsSetSoundEffect()
        {
            IsSoundEffectEnabled = AppSettingsService.Settings.IsEnableSoundEffects;

            if (IsSoundEffectEnabled)
                ChessSoundPlayer.PlayerNavSound();
        }
        
        /// <summary>
        /// Executes the cancel reset operation, showing a confirmation dialog and navigating to the home menu if confirmed.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecuteCancelReset(object? obj)
        {
            var panel = CustomDialogMessage.CustomDialogPanel("Cancel", "Are you sure you want to cancel?\nThis action will navigate to home menu!");
            var result = await DialogHost.Show(panel, "RootDialog");

            if ((bool?)result == true)
            {
                Reset(false);
                MainView.mainViewModel.Navigation?.NavigateTo<MainChessGameUI_VM>();
            }
        }

        /// <summary>
        /// Executes the reset to default operation, showing a confirmation dialog and resetting the settings to default values if confirmed.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecuteResetToDefault(object? obj)
        {

            var panel = CustomDialogMessage.CustomDialogPanel("Reset To Default", "Are you sure you want to reset settings to default?\nThis action can not be undone!");
            var result = await DialogHost.Show(panel, "RootDialog");

            // Reset to default colors and settings if the user confirms the action
            if ((bool?)result == true)
            {

                SelectedEvenSquareColor = new ColorItem("White", Colors.White, 137);
                SelectedEvenColorIndex = 137;

                SelectedOddSquareColor = new ColorItem("Black", Colors.Black, 7);
                SelectedOddColorIndex = 7;


                SelectedAvailableMovesColor = new ColorItem("Cyan", Colors.Cyan, 20);
                SelectedAvailableColorIndex = 20;

                SelectedAttackMovesColor = new ColorItem("DarkRed", Colors.DarkRed, 31);
                SelectedAttackColorIndex = 31;

                SelectedChessSet = "set1";
            }
        }

        /// <summary>
        /// Executes the reset setting service colors operation, showing a confirmation dialog and resetting the settings to the previous saved values if confirmed.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecuteResetSettingServiceColors(object? obj)
        {
            var panel = CustomDialogMessage.CustomDialogPanel("Reset Settings", "Are you sure you want to reset to the previous saved settings?\nThis can not be undone!");
            var result = await DialogHost.Show(panel, "RootDialog");

            if((bool?)result == true)
            {
                Reset();
            }
 
        }

        /// <summary>
        /// Executes the save changes and navigate home operation, showing a confirmation dialog and saving the current settings before navigating to the home menu if confirmed.
        /// </summary>
        /// <param name="obj">The command parameter.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecuteSaveChangesThenHome(object? obj)
        {
            var panel = CustomDialogMessage.CustomDialogPanel("Navigate Home", "Are you sure you want to save the current settings?\nSettings will be saved!");
            var result = await DialogHost.Show(panel, "RootDialog");

            try
            {
                if ((bool?)result == true)
                {
                    // Save the current settings to the application settings service
                    await SaveSettings();
                    MainView.mainViewModel.Navigation?.NavigateTo<MainChessGameUI_VM>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Loads the media colors from the Colors class and populates the MediaColors collection with ColorItem objects representing each color.
        /// </summary>
        public void LoadMediaColors()
        {
            // Get all public static properties of the Colors class using reflection
            var colorProperties = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static);

            int counter = 0;

            // Check if the application settings for square colors and indicators are not null before loading media colors
            if (AppSettingsService.Settings.squareOdd != null &&
                AppSettingsService.Settings.squareEven != null &&
                AppSettingsService.Settings.availableMovesIndicator != null &&
                AppSettingsService.Settings.attackMovesIndicator != null &&
                AppSettingsService.Settings.checkMateIndicator != null &&
                AppSettingsService.Settings.specialMoveIndicator != null)
            {
                // Iterate through each color property and create ColorItem objects for each color, adding them to the MediaColors collection
                foreach (var colorProperty in colorProperties)
                {
                    Object? value = colorProperty.GetValue(null, null);

                    // Check if the value is of type Color and create a ColorItem for it
                    if (value is Color color)
                    {
                        // Check if the color matches any of the application settings for square colors or indicators and create corresponding ColorItem objects
                        if (color == ((Color)AppSettingsService.Settings.squareOdd))
                        {
                            selectedOddSquareColor = new ColorItem(colorProperty.Name, (Color)AppSettingsService.Settings.squareOdd, counter);
                            OddSquareColor = new SolidColorBrush((Color)AppSettingsService.Settings.squareOdd);
                            selectedOddColorIndex = selectedOddSquareColor.Index;
                        }
                        else if (color == ((Color)AppSettingsService.Settings.squareEven))
                        {
                            selectedEvenSquareColor = new ColorItem(colorProperty.Name, (Color)AppSettingsService.Settings.squareEven, counter);
                            EvenSquareColor = new SolidColorBrush((Color)AppSettingsService.Settings.squareEven);
                            selectedEvenColorIndex = selectedEvenSquareColor.Index;
                        }
                        else if (color == (Color)AppSettingsService.Settings.availableMovesIndicator)
                        {
                            selectedAvailableMovesColor = new ColorItem(colorProperty.Name, (Color)AppSettingsService.Settings.availableMovesIndicator, counter);
                            AvailbleMovesIndicatorColor = new SolidColorBrush((Color)AppSettingsService.Settings.availableMovesIndicator);
                            selectedAvailableColorIndex = selectedAvailableMovesColor.Index;
                        }
                        else if (color == (Color)AppSettingsService.Settings.attackMovesIndicator)
                        {
                            selectedAttackMovesColor = new ColorItem(colorProperty.Name, (Color)AppSettingsService.Settings.attackMovesIndicator, counter);
                            AttackMovesIndicatorColor = new SolidColorBrush((Color)AppSettingsService.Settings.attackMovesIndicator);
                            selectedAttackColorIndex = selectedAttackMovesColor.Index;
                        }
                        else if(color == (Color)AppSettingsService.Settings.checkMateIndicator)
                        {
                            checKMateColor = new ColorItem(colorProperty.Name, (Color)AppSettingsService.Settings.checkMateIndicator, counter);
                        }
                        else if(color == (Color)AppSettingsService.Settings.specialMoveIndicator)
                        {
                            specialMoveColor = new ColorItem(colorProperty.Name, (Color)AppSettingsService.Settings.specialMoveIndicator, counter);
                        }

                        // Add the ColorItem to the MediaColors collection
                        MediaColors.Add(new ColorItem(colorProperty.Name, color, counter));
                        counter++;
                    }
                }
            }
        }

        /// <summary>
        /// Determines the selected chess set based on the application settings and sets the SelectedChessSet property accordingly.
        /// </summary>
        public void IsSelectedChessSet()
        {
            ArgumentNullException.ThrowIfNull(AppSettingsService.Settings.choosenChessSetSelection);

            if (AppSettingsService.Settings.choosenChessSetSelection.Equals("set1"))
                SelectedChessSet = "set1";
            else if (AppSettingsService.Settings.choosenChessSetSelection.Equals("set2"))
                SelectedChessSet = "set2";
            else if (AppSettingsService.Settings.choosenChessSetSelection.Equals("set3"))
                SelectedChessSet = "set3";
        }

        /// <summary>
        /// Shows the available moves and attack moves indicators on the chessboard for each chess piece.
        /// </summary>
        /// <param name="AvailableMovesIndicator">The color item representing the available moves indicator.</param>
        /// <param name="AttackMovesIndicator">The color item representing the attack moves indicator.</param>
        public void ShowAvailableMoves(ColorItem? AvailableMovesIndicator, ColorItem? AttackMovesIndicator)
        {
            if (SettingsChessBoard.ChessSquares_ != null)
            {
                // Iterate through each chess square on the chessboard
                foreach (var square in SettingsChessBoard.ChessSquares_)
                {
                    // Check if the square has a chess piece and if the piece has a valid image
                    if (square.ChessPiece_ != null && !string.IsNullOrWhiteSpace(square.ChessPiece_.PieceImage))
                    {
                        IChessMoves chessPiece = (IChessMoves)square.ChessPiece_;

                        // Check if the chess piece is not a Pawn before generating available moves and attack moves
                        if (chessPiece is not Pawn)
                        {
                            // generate available moves and attack moves for each chess piece on the board
                            chessPiece.GenerateAvailableMoves(SettingsChessBoard);
                            chessPiece.GenerateAttackMoves(SettingsChessBoard);

                            SettingsControls.AvailableMoveIndicator(SettingsChessBoard.ChessSquares_, square, AvailableMovesIndicator);
                            SettingsControls.AttackMoveIndicator(SettingsChessBoard.ChessSquares_, square, AttackMovesIndicator);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current settings to the application settings service, including square colors, available moves indicator color, 
        /// attack moves indicator color, selected chess set, and sound effects state.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveSettings()
        {
            AppSettingsService.Settings.squareOdd = SelectedOddSquareColor.Color;
            AppSettingsService.Settings.squareEven = SelectedEvenSquareColor.Color;
            AppSettingsService.Settings.availableMovesIndicator = SelectedAvailableMovesColor.Color;
            AppSettingsService.Settings.attackMovesIndicator = SelectedAttackMovesColor.Color;
            AppSettingsService.Settings.choosenChessSetSelection = SelectedChessSet;
            AppSettingsService.Settings.IsEnableSoundEffects = IsSoundEffectEnabled;

            // Save the updated settings asynchronously
            await AppSettingsService.SaveAsync();
        }

        /// <summary>
        /// Resets the settings to the previous saved values or default values based on the loadMediaColors parameter.
        /// </summary>
        /// <param name="loadMediaColors">A boolean value indicating whether to load media colors.</param>
        private void Reset(bool loadMediaColors = true)
        {
            
            if(loadMediaColors)
                LoadMediaColors();

            // Reset the selected chess set based on the application settings
            IsSelectedChessSet();

            // Reset the selected colors and sound effects state based on the application settings
            SelectedEvenColorIndex = selectedEvenSquareColor.Index;
            SelectedOddColorIndex = selectedOddSquareColor.Index;
            SelectedAvailableColorIndex = selectedAvailableMovesColor.Index;
            SelectedAttackColorIndex = selectedAttackMovesColor.Index;
            IsSoundEffectEnabled = AppSettingsService.Settings.IsEnableSoundEffects;

            // Reset the chessboard and show available moves and attack moves indicators
            SettingsChessBoard.DrawSettingsBoard(selectedOddSquareColor.Color, selectedEvenSquareColor.Color);

            // Show available moves and attack moves indicators
            ShowAvailableMoves(selectedAvailableMovesColor, selectedAttackMovesColor);
        }
    }
}
