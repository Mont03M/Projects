using ChessGame.Square;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ChessGame.Interfaces.Services;

namespace ChessGame.Utilities
{
    /// <summary>
    /// Represents a custom toggle button control that is used to toggle the state of a chess square in a chess game.
    /// </summary>
    public class ToggleChessSquare : ToggleButton
    {
        /// <summary>
        /// Gets the chess game service used for managing the state of the chess game.
        /// </summary>
        private readonly IIChessGameService? _myService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleChessSquare"/> class and retrieves the chess game service from the application services.
        /// </summary>
        public ToggleChessSquare()
        {
            _myService = App.Services?.GetRequiredService<IIChessGameService>();
        }

        /// <summary>
        /// Handles the click event of the toggle button. If the chess game is paused, a pawn promotion selection is active, or the game is over, the click event is ignored. 
        /// Otherwise, it finds and untoggles any other toggled buttons in the visual tree and then calls the base OnClick method.
        /// </summary>
        protected override void OnClick()
        {
            if ( _myService != null &&
                (_myService.IsGamePause ||
                _myService.IsPawnPromotionSelection || 
                _myService.IsGameOver))
                return;

            // Check if the DataContext is of type ChessSquare
            if (this.DataContext is ChessSquare square)
            {
                // Find the parent ItemsControl in the visual tree
                var parent = VisualTreeHelper.GetParent(this);

                // Traverse up the visual tree until we find an ItemsControl or reach the root
                while (parent != null && parent is not ItemsControl)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                // If we found an ItemsControl, call the FindToggledButtons method to untoggle other buttons
                FindToggledButtons(parent);

                // Call the base OnClick method to toggle the current button
                base.OnClick();
            }
        }

        /// <summary>
        /// Recursively searches the visual tree starting from the specified dependency object to find and untoggle any other toggled buttons of type ToggleChessSquare.
        /// </summary>
        /// <param name="dependencyObject">The starting dependency object for the search.</param>
        public void FindToggledButtons(DependencyObject? dependencyObject)
        {
            if (dependencyObject == null)
            {
                return;
            }

            // Iterate through the children of the dependency object
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                // Get the child dependency object at the specified index
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

                // If the child is a ToggleChessSquare and is not the current instance, untoggle it
                if (child is ToggleChessSquare toggle && toggle != this)
                {
                    toggle.IsChecked = false;
                }

                // Recursively search the children of the current child
                FindToggledButtons(child);
            }
        }
    }
}
