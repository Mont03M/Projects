using ChessGame.Enums;
using System.Collections.ObjectModel;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Defines the interface for a chess set selection, which includes dictionaries for white and black pieces and an observable collection for previewing the chess pieces.
    /// </summary>
    public interface IChessSetSelection
    {
        /// <summary>
        /// Gets or sets the dictionary of white pieces.
        /// </summary>
        public Dictionary<PieceType, string> whitePieces { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of black pieces.
        /// </summary>
        public Dictionary<PieceType, string> blackPieces { get; set; }

        /// <summary>
        /// Gets or sets the observable collection for previewing the chess pieces.
        /// </summary>  
        public ObservableCollection<string> ChessPiecesPreview { get; set; }

    }
}
