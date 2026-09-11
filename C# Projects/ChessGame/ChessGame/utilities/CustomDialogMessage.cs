using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChessGame.Utilities
{
    /// <summary>
    /// A static class that provides a method to create a custom dialog panel with a title, content message, and buttons.
    /// </summary>
    public static class CustomDialogMessage
    {
        /// <summary>
        /// Creates a custom dialog panel with the specified title, content message, and button labels.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="contentMessage">The content message of the dialog.</param>
        /// <param name="cancelButtonMsg">The label for the cancel button. Default is "Cancel".</param>
        /// <param name="restartButtonMsg">The label for the restart button. Default is "Ok".</param>
        /// <param name="transparency">Indicates whether the dialog should have a transparent background. Default is false.</param>
        /// <returns>A UIElement representing the custom dialog panel.</returns>
        public static UIElement CustomDialogPanel(string title, string contentMessage, string cancelButtonMsg = "Cancel", string restartButtonMsg = "Ok", bool transparency = false)
        {
            // Create a Border to serve as the dialog panel
            var panel = new Border
            {
                Width = 380,
                CornerRadius = new CornerRadius(12),
                Background =  (transparency) ? new SolidColorBrush(Color.FromArgb(64, 36, 36, 36)) : new SolidColorBrush(Color.FromRgb(36, 36, 36)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(25)
            };

            // Create a StackPanel to hold the content of the dialog
            var content = new StackPanel();

            // Add the title TextBlock to the content StackPanel
            content.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Add the content message TextBlock to the content StackPanel
            content.Children.Add(new TextBlock
            {
                Text = contentMessage,
                FontSize = 15,
                Foreground = (transparency) ? Brushes.White : Brushes.Gainsboro,
                Margin = new Thickness(0, 15, 0, 25),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });

            // Create a StackPanel to hold the buttons
            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create the cancel button with the specified label and styling
            var cancelButton = new Button
            {
                Content = cancelButtonMsg,
                Width = 120,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background =  (transparency) ? new SolidColorBrush(Color.FromArgb(64, 70, 70, 70)) : new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = false
            };

            // Create the restart button with the specified label and styling
            var restartButton = new Button
            {
                Content = restartButtonMsg,
                Width = 120,
                Height = 40,
                Background = (transparency) ? new SolidColorBrush(Color.FromArgb(64, 191, 137, 66)) : new SolidColorBrush(Color.FromRgb(191, 137, 66)), // chess gold
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.Bold,
                Command = DialogHost.CloseDialogCommand,
                CommandParameter = true
            };

            // Add the buttons to the buttons StackPanel
            buttons.Children.Add(cancelButton);
            buttons.Children.Add(restartButton);

            // Add the buttons StackPanel to the content StackPanel
            content.Children.Add(buttons);

            // Set the content of the dialog panel to the content StackPanel
            panel.Child = content;

            return panel;
        }
    }
}
