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

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SwitchControl.Content is ChessMatch chessMatch && chessMatch.DataContext is ChessMatchVM vm)
            {
                vm.ChessGame.StockFishEval?.Dispose();
            }

            await this.appSettingsService.SaveAsync();
        }
    }
}