using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Structs;

namespace ChessGame.ChessGameMoves.PeekMoves
{
    /// <summary>
    /// Peek class represents a potential move in a chess game, encapsulating the move's destination, the type of piece making the move, and the color of that piece.
    /// </summary>
    /// <param name="move">The location of the move.</param>
    /// <param name="pieceType">The type of chess piece.</param>
    /// <param name="color">The color of the chess piece.</param>
    public class Peek(ChessSquareLocation move, PieceType pieceType, ChessPieceColors color) : IChessMove
    {
        public ChessSquareLocation Move { get; set; } = move;

        public PieceType PieceType { get; set; } = pieceType;

        public ChessPieceColors Color { get; set; } = color;
    }
}
