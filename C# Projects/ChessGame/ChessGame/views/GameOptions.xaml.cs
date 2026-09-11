using ChessGame.Game.chessSoundPlayer;
using ChessGame.Services;
using ChessGame.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace ChessGame.Views
{
    /// <summary>
    /// Interaction logic for GameOptions.xaml
    /// </summary>
    public partial class GameOptions : UserControl
    {
        /// <summary>
        /// The AppSettingsService instance used to access application settings.
        /// </summary> 
        private readonly AppSettingsService _appSettings;

        /// <summary>
        /// Initializes a new instance of the GameOptions class.
        /// </summary>
        public GameOptions() : this(App.Services.GetRequiredService<GameOptionsVM>()){}

        public GameOptions(GameOptionsVM vm)
        {
            InitializeComponent();

            // Resolve the service from the host IServiceProvider (App should expose it)
            // Replace 'App' and 'ServiceProvider' with your actual access pattern
            DataContext = vm ?? throw new ArgumentNullException(nameof(vm));

            // Initialize the AppSettingsService
            _appSettings = vm.appSettingsService;

            if(_appSettings.Settings.IsEnableSoundEffects)
               ChessSoundPlayer.PlayerNavSound();
            
        }
    }
}
