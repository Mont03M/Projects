using ChessGame.Enums;

namespace ChessGame.Strategies.Tools.StockfishTools
{
    /// <summary>
    /// Represents information about a chess move, including the type of piece and the type of move. 
    /// This class is used in conjunction with Stockfish chess engine tools to analyze and evaluate chess moves.
    /// </summary>
    /// <param name="pieceType"></param>
    /// <param name="moveType"></param>
    public class StockChessMoveInfo(PieceType? pieceType, MoveType? moveType)
    {
        /// <summary>
        /// Gets or sets the type of chess piece involved in the move. This property is nullable to accommodate moves that may not involve a specific piece type.
        /// </summary>
        public PieceType? ChessPieceType { get; set; } = pieceType;

        /// <summary>
        /// Gets or sets the type of chess move. This property is nullable to accommodate moves that may not have a specific move type.
        /// </summary>
        public MoveType? ChessMoveType { get; set; } = moveType;
    }
}
