using ChessGame.ChessPieces;
using ChessGame.Square;
using ChessGame.Enums;
using ChessGame.Structs;
using ChessGame.Interfaces.Game;

namespace ChessGame.ChessGameMoves.Moves
{
    /// <summary>
    /// Class represents a move that is available for a chess piece. It contains information about the type of move, 
    /// the type of piece, the location of the move, the owner of the move, and references to the chess piece and squares involved in the move.
    /// </summary>
    /// <param name="moveType">The type of move.</param>
    /// <param name="pieceType">The type of chess piece.</param>
    /// <param name="move">The location of the move.</param>
    /// <param name="moveOwner">The owner of the move.</param>
    /// <param name="chessPieceRef">Reference to the chess piece involved in the move.</param>
    /// <param name="squareOne">The first square involved in the move.</param>
    /// <param name="squareTwo">The second square involved in the move.</param>
    public class MovesAvailable(MoveType moveType, PieceType pieceType, ChessSquareLocation move, ChessPiece? moveOwner,
        ChessPiece chessPieceRef = null!, ChessSquare squareOne = null!, ChessSquare squareTwo = null!) : IChessMove
    {
        public MoveType MoveType { get; set; } = moveType;

        public PieceType PieceType { get; set; } = pieceType;

        public ChessSquareLocation Move { get; set; } = move;

        public ChessPiece? MoveOwner { get; set; } = moveOwner;

        public ChessPiece ChessPieceRef { get; set; } = chessPieceRef;

        public ChessSquare SquareOne { get; set; } = squareOne;

        public ChessSquare SquareTwo { get; set; } = squareTwo;
    }
}
