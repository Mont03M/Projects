using System.Windows.Controls;

namespace ChessGame.Utilities
{
    /// <summary>
    /// A custom RadioButton control that allows toggling its checked state when clicked, even if it is already checked.
    /// </summary>
    public class ToggleabeRadioButton : RadioButton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleabeRadioButton"/> class.
        /// </summary>
        protected override void OnClick()
        {
            if(this.IsChecked == true)
                IsChecked = false;
            else
                base.OnClick();
        }
    }
}
