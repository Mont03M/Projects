using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using System.Collections.ObjectModel;

namespace ChessGame.ChessSets
{
    /// <summary>
    /// Represents the second chess set selection with specific piece images for white and black pieces.
    /// </summary>
    public class ChessSet2 : IChessSetSelection
    {
        public Dictionary<PieceType, string> whitePieces { get; set; }
        public Dictionary<PieceType, string> blackPieces { get; set; }
        public ObservableCollection<string> ChessPiecesPreview { get; set; }

        /// <summary>
        /// Initializes a new instance of the ChessSet2 class, setting up the file paths for the images of white and black chess pieces.
        /// </summary>
        public ChessSet2()
        {

            whitePieces = new Dictionary<PieceType, string>
            {
                {PieceType.PAWN, "/assets/chessPieceSets/ChessPiecesSet2/pawn_white.png"},
                {PieceType.BISHOP, "/assets/chessPieceSets/ChessPiecesSet2/bishop_white.png"},
                {PieceType.KNIGHT,"/assets/chessPieceSets/ChessPiecesSet2/knight_white.png" },
                {PieceType.ROOK, "/assets/chessPieceSets/ChessPiecesSet2/rook_white.png" },
                {PieceType.QUEEN, "/assets/chessPieceSets/ChessPiecesSet2/queen_white.png"},
                {PieceType.KING, "/assets/chessPieceSets/ChessPiecesSet2/king_white.png" },
            };

            blackPieces = new Dictionary<PieceType, string>
            {
                {PieceType.PAWN, "/assets/chessPieceSets/ChessPiecesSet2/pawn_black.png"},
                {PieceType.BISHOP, "/assets/chessPieceSets/ChessPiecesSet2/bishop_black.png"},
                {PieceType.KNIGHT,"/assets/chessPieceSets/ChessPiecesSet2/knight_black.png" },
                {PieceType.ROOK, "/assets/chessPieceSets/ChessPiecesSet2/rook_black.png" },
                {PieceType.QUEEN, "/assets/chessPieceSets/ChessPiecesSet2/queen_black.png"},
                {PieceType.KING, "/assets/chessPieceSets/ChessPiecesSet2/king_black.png" },
            };

            ChessPiecesPreview = ChessPiecePreview();
        }

        /// <summary>
        /// Generates a preview of the chess pieces by combining the file paths of white and black pieces into an ObservableCollection.
        /// </summary>
        /// <returns> An ObservableCollection containing the file paths of the white and black chess pieces. </returns>
        public ObservableCollection<string> ChessPiecePreview()
        {
            ObservableCollection<string> chessPieces = [.. whitePieces.Values.Concat(blackPieces.Values).ToList()];

            return chessPieces;
        }
    }
}
