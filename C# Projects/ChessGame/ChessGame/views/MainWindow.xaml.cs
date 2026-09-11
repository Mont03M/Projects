using ChessGame.Services;
using ChessGame.ViewModels;
using ChessGame.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettingsService appSettingsService;

        public MainWindow(AppSettingsService appSettingsService)
        {
            InitializeComponent();
            this.appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Handles the window closing event. Disposes of the StockFishEval object if it exists and saves application settings asynchronously.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Dispose of the StockFishEval object if it exists
            if (SwitchControl.Content is ChessMatch chessMatch && chessMatch.DataContext is ChessMatchVM vm)
            {
                vm.ChessGame.StockFishEval?.Dispose();
            }

            // Save application settings asynchronously
            await this.appSettingsService.SaveAsync();
        }
    }
}