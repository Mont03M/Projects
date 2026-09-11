using ChessGame.Game.chessSoundPlayer;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Utilities;
using ChessGame.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// The ServiceProvider instance used for dependency injection and service resolution.
        /// </summary>
        private readonly ServiceProvider _serviceProvider;

        /// <summary>
        /// Gets the IServiceProvider instance for accessing registered services and view models.
        /// </summary>
        public static IServiceProvider? Services { get; private set; }

        /// <summary>
        /// Creates the services needed for the application to share data across viewModels
        /// </summary>
        public App()
        {
            // Create a new ServiceCollection to register services and view models
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<MainWindow>(provider => new MainWindow(
                provider.GetRequiredService<AppSettingsService>())
            {
                DataContext = provider.GetRequiredService<MainViewModelBase>()
            });

            // Register view models and services with appropriate lifetimes
            services.AddSingleton<MainViewModelBase>(); // Register MainViewModelBase as a singleton
            services.AddTransient<MainChessGameUI_VM>(); // Register MainChessGameUI_VM as transient
            services.AddTransient<ChessMatchVM>(); // Register ChessMatchVM as transient
            services.AddSingleton<GameOptionsVM>(); // Register GameOptionsVM as a singleton
            services.AddSingleton<SettingsVM>(); // Register SettingsVM as a singleton
            services.AddSingleton<AppSettingsService>(); // Register AppSettingsService as a singleton
            services.AddSingleton<IIObjectService, IObjectService>(); // Register IIObjectService with its implementation IObjectService as a singleton
            services.AddSingleton<IISettingsService, ISettingsService>(); // Register IISettingsService with its implementation ISettingsService as a singleton
            services.AddSingleton<IIChessGameService, IChessGameService>(); // Register IIChessGameService with its implementation IChessGameService as a singleton
            services.AddSingleton<INavigationService, NavigationService>(); // Register INavigationService with its implementation NavigationService as a singleton

            // Register a factory for creating ViewModel instances based on their type
            services.AddSingleton<Func<Type, Utilities.ViewModel>>
                (serviceProvider => viewModelType => (Utilities.ViewModel)serviceProvider.GetRequiredService(viewModelType));

            // Register the ViewModelFactory as a singleton
            Services = _serviceProvider = services.BuildServiceProvider();
        }
      
        /// <summary>
        /// Starts the applications with services and DI 
        /// </summary>
        /// <param name="e">The StartupEventArgs instance containing the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load the application settings before showing the main window
            var appSettingService = _serviceProvider.GetRequiredService<AppSettingsService>();

            // Load the settings asynchronously
            await appSettingService.LoadAsync();

            // Show the main window after loading the settings
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            if(appSettingService.Settings.IsEnableSoundEffects)
                ChessSoundPlayer.PlayIntro();
        }
    }

}
