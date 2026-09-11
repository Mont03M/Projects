using ChessGame.ChessPieces;
using ChessGame.Structs;
using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.PeekMoves;

namespace ChessGame.Interfaces.Game
{
    /// <summary>
    /// Defines the interface for managing chess moves, including generating available, attack, and valid moves, as well as handling special move scenarios like checks. 
    /// This interface extends IChessMovesLists and provides methods for adding, replacing, and retrieving available moves in a chess game.
    /// </summary>
    public interface IChessMoves : IChessMovesLists
    {
        /// <summary>
        /// Generates the available moves for the chess piece on the given chess board. 
        /// This method should be implemented to calculate all possible moves based on the current state of the board and the rules of chess.
        /// </summary>
        /// <param name="chessBoard"></param>
        void GenerateAvailableMoves(Board chessBoard);

        /// <summary>
        /// Generates the attack moves for the chess piece on the given chess board.
        /// </summary>
        /// <param name="chessBoard">The current state of the chessboard.</param>
        void GenerateAttackMoves(Board chessBoard);

        /// <summary>
        /// Generates the moves that put the opponent's king in check.
        /// </summary>
        /// <param name="attacker">The chess piece that is attacking.</param>
        /// <param name="pathToKing">The path to the opponent's king.</param>
        /// <param name="chessBoard">The current state of the chessboard.</param>
        void GenerateIsCheckMoves(ChessPiece attacker, List<ChessSquareLocation>? pathToKing, Board chessBoard = null!);

        /// <summary>
        /// Generates the valid moves for the chess piece on the given chess board, considering the current game state and rules of chess.
        /// </summary>
        /// <param name="chessBoard">The current state of the chessboard.</param>
        void GenerateValidMoves(Board chessBoard);

        /// <summary>
        /// Adds an available peek move.
        /// </summary>
        /// <param name="p">The peek move to add.</param>
        public void AddAvailablePeek(Peek? p);

        /// <summary>
        /// Replaces all available moves with the given items.
        /// </summary>
        /// <param name="items">The new set of available moves.</param>
        public void ReplaceAllAvailableMoves(IEnumerable<Peek>? items);

        /// <summary>
        /// Gets a snapshot of all available moves.
        /// </summary>
        /// <returns>A list of all available peek moves.</returns>
        public List<Peek> GetAllAvailableMovesSnapshot();
        
    }
}
