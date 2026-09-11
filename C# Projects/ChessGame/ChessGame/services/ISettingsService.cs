using ChessGame.ChessSets;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using System.Windows.Media;

namespace ChessGame.Services
{
    /// <summary>
    /// Represents the settings service for the chess game, providing properties for various game settings such as square colors, 
    /// chess set selection, move indicators, and sound effects.
    /// </summary>
    public class ISettingsService : IISettingsService
    {
        /// <summary>
        /// Gets or sets the color of the odd squares on the chessboard.
        /// </summary>
        public Color? squareOdd { get; set; } = Colors.Black;

        /// <summary>
        /// Gets or sets the color of the even squares on the chessboard.
        /// </summary>
        public Color? squareEven { get; set; } = Colors.White;

        /// <summary>
        /// Gets or sets the selected chess set.
        /// </summary>
        public string choosenChessSetSelection { get; set; } = new string("set1");

        /// <summary>
        /// Gets or sets the color of the available moves indicator.
        /// </summary>
        public Color? availableMovesIndicator { get; set; } = Colors.Cyan;

        /// <summary>
        /// Gets or sets the color of the attack moves indicator.
        /// </summary>
        public Color? attackMovesIndicator { get; set; } = Colors.DarkRed;

        /// <summary>
        /// Gets or sets the color of the checkmate indicator.
        /// </summary>
        public Color? checkMateIndicator { get; set; } = Colors.Goldenrod;

        /// <summary>
        /// Gets or sets the color of the special move indicator.
        /// </summary>
        public Color? specialMoveIndicator { get; set; } = Colors.Green;

        /// <summary>
        /// Gets or sets the color of the rank and file labels.
        /// </summary>
        public Color? RankFileColor { get; set; } = Colors.Silver;

        /// <summary>
        /// Gets or sets a value indicating whether the available moves indicator is enabled.
        /// </summary>
        public bool IsEnableAvailableMovesIndicator { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the attack moves indicator is enabled.
        /// </summary>
        public bool IsEnableAttackMovesIndicator { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the checkmate indicator is enabled.
        /// </summary>
        public bool IsEnableCheckMateIndicator { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the special move indicator is enabled.
        /// </summary>
        public bool IsEnableSpecialMoveIndicator { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the sound effects are enabled.
        /// </summary>
        public bool IsEnableSoundEffects { get; set; } = true;

        /// <summary>
        /// Gets the selected chess set.
        /// </summary>
        /// <returns>The selected chess set.</returns>
        public IChessSetSelection ChessSetSelected()
        {
            return choosenChessSetSelection switch
            {
                "set1" => new ChessSet1(),
                "set2" => new ChessSet2(),
                "set3" => new ChessSet3(),
                _ => new ChessSet1()
            };
        }
    }
}
