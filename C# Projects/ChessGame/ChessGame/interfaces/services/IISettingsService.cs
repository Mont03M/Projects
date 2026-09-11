using ChessGame.Interfaces.Game;
using System.Windows.Media;

namespace ChessGame.Interfaces.Services
{
    /// <summary>
    /// Defines the interface for a settings service that manages various game settings, including colors for chessboard squares, indicators for moves, and sound effects. 
    /// This interface provides properties to get and set these settings, as well as a method to retrieve the selected chess set.
    /// </summary>
    public interface IISettingsService
    {
        /// <summary>
        /// Gets or sets the color for odd squares on the chessboard.
        /// </summary>
        public Color? squareOdd { get; set; }

        /// <summary>
        /// Gets or sets the color for even squares on the chessboard.
        /// </summary>
        public Color? squareEven { get; set; }

        /// <summary>
        /// Gets or sets the chosen chess set selection.
        /// </summary>
        public string choosenChessSetSelection { get; set; }

        /// <summary>
        /// Gets or sets the color for available moves indicator.
        /// </summary>
        public Color? availableMovesIndicator { get; set; }

        /// <summary>
        /// Gets or sets the color for attack moves indicator.
        /// </summary>
        public Color? attackMovesIndicator { get; set; }

        /// <summary>
        /// Gets or sets the color for checkmate indicator.
        /// </summary>
        public Color? checkMateIndicator { get; set; }

        /// <summary>
        /// Gets or sets the color for special move indicator.
        /// </summary>
        public Color? specialMoveIndicator { get; set; }

        /// <summary>
        /// Gets or sets the color for rank and file labels.
        /// </summary>
        public Color? RankFileColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the available moves indicator is enabled.
        /// </summary>
        public bool IsEnableAvailableMovesIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the attack moves indicator is enabled.
        /// </summary>
        public bool IsEnableAttackMovesIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the checkmate indicator is enabled.
        /// </summary>
        public bool IsEnableCheckMateIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the special move indicator is enabled.
        /// </summary>
        public bool IsEnableSpecialMoveIndicator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sound effects are enabled.
        /// </summary>
        public bool IsEnableSoundEffects { get; set; }

        /// <summary>
        /// Gets the selected chess set.
        /// </summary>  
        public IChessSetSelection ChessSetSelected();

    }
}
