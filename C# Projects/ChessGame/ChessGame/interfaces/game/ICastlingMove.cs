using ChessGame.ChessBoard;
using ChessGame.ChessPieces;
using ChessGame.Structs;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Defines an interface for generating castling moves in a chess game. Implementing classes should provide the logic to generate valid castling moves based 
    /// on the current state of the chessboard, the positions of the king and rooks, and any squares that are attacked.
    /// </summary>
    public interface ICastlingMove
    {
        /// <summary>
        /// Generates valid castling moves based on the current state of the chessboard, the positions of the king and rooks, and any squares that are attacked.
        /// </summary>
        /// <param name="chessBoard">The current state of the chessboard.</param>
        /// <param name="king">The king involved in the castling move.</param>
        /// <param name="rookQS">The queen-side rook involved in the castling move.</param>
        /// <param name="rookKS">The king-side rook involved in the castling move.</param>
        /// <param name="SquaresAttacked">A list of squares that are attacked, along with the corresponding rook locations.</param>
        public void GenerateCasltingMove(Board chessBoard, King? king, Rook? rookQS, Rook? rookKS, 
            List<(ChessSquareLocation AttackedSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked);
    }
}
