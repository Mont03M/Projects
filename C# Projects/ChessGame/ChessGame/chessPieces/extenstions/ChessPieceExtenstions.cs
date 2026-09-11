using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.PeekMoves;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Square;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ChessGame.ChessPieces.Extenstions
{
    /// <summary>
    /// Provides extension methods and utility functions for chess pieces, including move generation, piece finding, and cloning.
    /// </summary>
    public class ChessPieceExtenstions
    {
        #region -- Chess Square Location Methods --
        /// <summary>
        /// Returns a new ChessSquareLocation that is one square forward from the given coordinates (x, y).
        /// </summary>
        /// <param name="x"> int type x-axis </param>
        /// <param name="y"> int type y-axis </param>
        /// <returns> a new ChessSquareLocation representing the square one move forward </returns>
        public static ChessSquareLocation OneSquareForward(int x, int y)
        {
            return new ChessSquareLocation((int)x, (int)y);
        }

        /// <summary>
        /// Returns a new ChessSquareLocation that is one square diagonal from the given coordinates (x, y).
        /// </summary>
        /// <param name="x"> int type x-axis </param>
        /// <param name="y"> int type y-axis </param>
        /// <returns> a new ChessSquareLocation representing the square one move diagonal </returns>
        public static ChessSquareLocation OneSquareDiagonal(int x, int y)
        {
            return new ChessSquareLocation((int)x, (int)y);
        }

        /// <summary>
        /// Finds a chess piece of the specified type and color on the given collection of chess squares.
        /// </summary>
        /// <param name="chessPiece">The type of chess piece to find.</param>
        /// <param name="color">The color of the chess piece to find.</param>
        /// <param name="squares">The collection of chess squares to search.</param>
        /// <returns>The found chess piece, or null if not found.</returns>
        public static IChessMoves? FindChessPiece(PieceType chessPiece, ChessPieceColors color,
            ObservableCollection<ChessSquare> squares)
        {
            // loop through each square on the chess board
            foreach (var square in squares)
            {
                if (square.ChessPiece_ != null)
                {
                    // check if the chess piece matches the specified type and color
                    if (square.ChessPiece_.PieceType == chessPiece && square.ChessPiece_.PieceColor.Equals(color))
                    {
                        // return the chess piece if found
                        return (IChessMoves)square.ChessPiece_;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the type of chess piece located at the specified location on the chess board.
        /// </summary>
        /// <param name="location">The location of the square to check.</param>
        /// <param name="chessBoard">The collection of chess squares to search.</param>
        /// <returns>The type of chess piece at the specified location, or null if no piece is found.</returns>
        public static PieceType? GetChessPieceType(ChessSquareLocation location, ObservableCollection<ChessSquare> chessBoard)
        {
            foreach (var square in chessBoard)
            {
                // check if the square contains a chess piece and if its current location matches the specified location
                if (square.ChessPiece_ is ChessPiece chessPiece && chessPiece.CurrentLocation == location)
                {
                    return chessPiece.PieceType;
                }
            }

            return null;
        }

        /// <summary>
        /// Generates a collection of chess pieces that can be promoted to when a pawn reaches the opposite end of the board.
        /// </summary>
        /// <param name="chessPieces">The type of chessPiece to retrieve </param>
        /// <param name="pawn"> a pawn </param>
        /// <param name="location"> an (x,y) location </param>
        /// <returns></returns>
        public static IEnumerable<ChessPiece> PawnPromotionChessPieces(Dictionary<PieceType, string> chessPieces,
          Pawn pawn, ChessSquareLocation location)
        {
            foreach (var piece in chessPieces)
            {
                // Exclude pawns and kings from the promotion options
                if (piece.Key is not PieceType.PAWN and not PieceType.KING)
                {
                    // Create a new chess piece for promotion and yield return it
                    yield return new ChessPiece(
                        piece.Key,
                        piece.Value,
                        pawn,
                        location);
                }
            }
        }

        /// <summary>
        /// Gets the opposite color of the given chess piece color. If the input color is WHITE, it returns BLACK; otherwise, it returns WHITE.
        /// </summary>
        /// <param name="color"> The color for which to get the opposite </param>
        /// <returns> The opposite color </returns>
        public static ChessPieceColors GetOtherColor(ChessPieceColors color)
        {
            return (color == ChessPieceColors.WHITE) ? ChessPieceColors.BLACK : ChessPieceColors.WHITE;
        }

        #endregion

        #region -- Chess Piece Move Generation Methods --

        /// <summary>
        /// Gets all possible moves for the chess pieces on the board.
        /// </summary>
        /// <param name="allMoves">The collection of chess squares to search.</param>
        /// <returns>A list of all possible moves.</returns>
        public static List<Peek> AllMoves(ObservableCollection<ChessSquare> allMoves)
        {
            List<Peek> moves = [];

            // loop through each chess pieces possible moves 
            foreach (var piece in allMoves)
            {
                if (piece.ChessPiece_ != null)
                {
                    // cast object
                    IChessMoves chessPiece = (IChessMoves)piece.ChessPiece_;

                    if (chessPiece.GetAllAvailableMovesSnapshot() != null)
                    {
                        // add to existing moves
                        moves = ConcatList(moves, chessPiece.GetAllAvailableMovesSnapshot());
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Gets all attack moves for the specified color on the given board. Optionally generates new moves and retrieves available moves.
        /// </summary>
        /// <typeparam name="T">The type of moves to retrieve.</typeparam>
        /// <param name="boardCopy">The copy of the chess board.</param>
        /// <param name="color">The color of the pieces for which to get attack moves.</param>
        /// <param name="GeneratingNewMoves">Indicates whether to generate new moves.</param>
        /// <param name="GetMovesAvailable">Indicates whether to get available moves.</param>
        /// <returns>A list of all attack moves for the specified color.</returns>
        public static List<T>? GetAllAttackMoves<T>(Board boardCopy, ChessPieceColors color, bool GeneratingNewMoves = false, bool GetMovesAvailable = false)
        {
            var movesToGet = new List<T>();

            // check if the board copy and its chess squares are not null
            if (boardCopy != null && boardCopy.ChessSquares_ != null)
            {
                // generate new moves if specified
                if (GeneratingNewMoves)
                {
                    foreach (var chessPiece in boardCopy.ChessSquares_)
                    {
                        // check if the chess piece is not null, matches the specified color, and implements IChessMoves
                        if (chessPiece.ChessPiece_ != null &&
                            chessPiece.ChessPiece_.PieceColor == color &&
                            chessPiece.ChessPiece_ is IChessMoves chessMoves)
                        {
                            // generate available and attack moves for the chess piece
                            chessMoves.GenerateAvailableMoves(boardCopy);
                            chessMoves.GenerateAttackMoves(boardCopy);
                        }
                    }
                }

                foreach (var square_ in boardCopy.ChessSquares_.ToList())
                {
                    // check if the chess piece is not null, implements IChessMoves, and matches the specified color
                    if (square_.ChessPiece_ != null &&
                        square_.ChessPiece_ is IChessMoves chessMoves &&
                        square_.ChessPiece_.PieceColor == color)
                    {
                        // check if the chess piece has attack moves and add them to the list of moves to get
                        if (chessMoves.AttackMoves != null)
                        {
                            foreach (var move in chessMoves.AttackMoves.ToList())
                            {
                                // add the move to the list of moves to get based on the specified type
                                if (GetMovesAvailable)
                                    movesToGet.Add((T)(object)move);
                                else
                                    movesToGet.Add((T)(object)move.Move);
                            }
                        }
                    }
                }
            }

            return movesToGet;
        }

        /// <summary>
        /// Gets all available moves for the specified color on the given chess board. Optionally retrieves move types and special moves.
        /// </summary>
        /// <typeparam name="T"> The type of moves to retrieve.</typeparam>
        /// <param name="chessBoard"> The chess board to retrieve moves from.</param>
        /// <param name="pieceColorToGet"> The color of the pieces for which to retrieve moves.</param>
        /// <param name="GetMoveTypes"> Indicates whether to retrieve move types.</param>
        /// <param name="GetSpecialMoves"> Indicates whether to retrieve special moves.</param>
        /// <returns>A list of available moves for the specified color.</returns>
        public static List<T> GetAvailableMoves<T>(List<ChessSquare> chessBoard,
            ChessPieceColors pieceColorToGet, bool GetMoveTypes = false, bool GetSpecialMoves = true)
        {
            var movesToGet = new List<T>();

            foreach (var square in chessBoard)
            {
                if (square.ChessPiece_ != null && square.ChessPiece_.PieceColor == pieceColorToGet)
                {
                    // check if the chess piece implements IChessMoves and has available moves
                    if (square.ChessPiece_ is IChessMoves moves &&
                        moves.AvailableMoves != null)
                    {
                        // take a snapshot of the available moves to avoid potential threading issues
                        List<MovesAvailable>? snapshot = null;
                        lock (SyncRoot.MovesLock)
                        {
                            snapshot = moves.AvailableMoves?.ToList();
                        }

                        // add the available moves to the list of moves to get based on the specified type
                        if (snapshot != null)
                        {
                            foreach (var location in snapshot)
                            {
                                if (GetMoveTypes)
                                    movesToGet.Add((T)(object)location);
                                else
                                    movesToGet.Add((T)(object)location.Move);
                            }
                        }
                    }

                    // check if the chess piece implements IPieceSpecialMove and has special moves
                    if (GetSpecialMoves &&
                        GetMoveTypes &&
                           square.ChessPiece_ is IPieceSpecialMove pieceSpecialMoves &&
                           pieceSpecialMoves.SpecialMoves != null &&
                           pieceSpecialMoves.SpecialMoves.Count > 0)
                    {
                        // take a snapshot of the special moves to avoid potential threading issues
                        List<MovesAvailable>? specialSnapshot = null;
                        lock (SyncRoot.MovesLock)
                        {
                            specialSnapshot = pieceSpecialMoves.SpecialMoves?.ToList();
                        }

                        // add the special moves to the list of moves to get based on the specified type
                        if (specialSnapshot != null)
                        {
                            foreach (var move in specialSnapshot)
                            {
                                // add the move to the list of moves to get based on the specified type
                                if (GetMoveTypes)
                                    movesToGet.Add((T)(object)move);
                                else
                                    movesToGet.Add((T)(object)move.Move);
                            }
                        }
                    }
                }
            }

            return movesToGet;
        }



        /// <summary>
        /// Gets all available moves for the specified player color on the given chess board. Optionally includes pawn possible attack moves.
        /// </summary>
        /// <typeparam name="T">The type of moves to retrieve.</typeparam>
        /// <param name="chessBoard">The chess board to retrieve moves from.</param>
        /// <param name="playerColor">The color of the player for whom to retrieve moves.</param>
        /// <param name="includePawnPossibleAttackMoves">Indicates whether to include pawn possible attack moves.</param>
        /// <returns>A list of available moves for the specified player color.</returns>
        public static List<T> GetMoves<T>(Board chessBoard, ChessPieceColors playerColor, bool includePawnPossibleAttackMoves = false)
        {
            // Get all available moves for the specified player color
            var availableMoves = chessBoard.ChessSquares_?
                .Where(x => x.ChessPiece_ is not null && x.ChessPiece_.PieceColor == playerColor)
                .Select(x => x.ChessPiece_)
                .OfType<IChessMoves>()
                .SelectMany(x =>{
                    var list = x.AvailableMoves;
                    return list == null ? Enumerable.Empty<MovesAvailable>() : [.. list];
                }).ToList();

            // Get all attack moves for the specified player color
            var attackMoves = chessBoard.ChessSquares_?
                .Where(x => x.ChessPiece_ is not null && x.ChessPiece_.PieceColor == playerColor)
                .Select(x => x.ChessPiece_)
                .OfType<IChessMoves>()
                .SelectMany(x => { 
                    var list = x.AttackMoves;
                    return list == null ? Enumerable.Empty<MovesAvailable>() : [.. list];
                }).ToList();

            // Get all special moves for the specified player color
            var specialMoves = chessBoard.ChessSquares_?
                .Where(x => x.ChessPiece_ is not null && x.ChessPiece_.PieceColor == playerColor)
                .Select(x => x.ChessPiece_)
                .OfType<IPieceSpecialMove>()
                .SelectMany(x => {
                    var list = x.SpecialMoves;
                    return list == null ? Enumerable.Empty<MovesAvailable>() : [.. list];
                }).ToList();

            // Combine the available moves, attack moves, and special moves into a single list of moves
            if (includePawnPossibleAttackMoves)
            {
                // Get all possible attack moves for pawns of the specified player color
                var pawnPossibleAttackMoves = chessBoard.ChessSquares_?
                    .Where(x => x.ChessPiece_ is not null && 
                           x.ChessPiece_.PieceColor == playerColor && 
                           x.ChessPiece_.PieceType is PieceType.PAWN)
                    .Select(x => x.ChessPiece_)
                    .OfType<IChessMoves>()
                    .SelectMany(x => 
                    {
                        var list = x.GetAllAvailableMovesSnapshot();
                        return list == null ? Enumerable.Empty<Peek>() : [.. list];
                    })
                    .Where(pfm => pfm != null)
                    .ToList();

                // Remove available moves that also exist as attack moves
                availableMoves?.RemoveAll(
                    availableMove => attackMoves is not null
                    && attackMoves.Any(attackMove => attackMove.Equals(availableMove)));

                // Remove available moves that also exist as pawn possible attack moves
                availableMoves?.RemoveAll(
                    availableMoves => pawnPossibleAttackMoves is not null &&
                    pawnPossibleAttackMoves
                    .Any(pfm => pfm.Move == availableMoves.Move));

                // Remove attack moves that also exist as pawn possible attack moves
                attackMoves?.RemoveAll(
                    attackMove => pawnPossibleAttackMoves is not null &&
                    pawnPossibleAttackMoves
                    .Any(pfm => pfm.Move == attackMove.Move));

                // Combine them, with attack moves taking priority
                var moves = attackMoves?.Select(x => x.Move)?
                    .Concat(availableMoves?.Select(x => x.Move) ?? [])
                    .Concat(specialMoves?.Select(x => x.Move) ?? [])
                    .Concat(pawnPossibleAttackMoves?.Select(x => x.Move) ?? [])
                    .ToList();

                // Return the combined moves as a list of type T if there are any moves available
                if (moves?.Count > 0)
                {
                    return [.. moves.Cast<T>()];
                }
            }
            else
            {

                // Remove available moves that also exist as attack moves
                availableMoves?.RemoveAll(
                    availableMove => attackMoves is not null
                    && attackMoves.Any(attackMove => attackMove.Equals(availableMove)));

                // Combine them, with attack moves taking priority
                var moves = attackMoves?.Concat(availableMoves ?? []).Concat(specialMoves ?? []).ToList();

                // Return the combined moves as a list of type T if there are any moves available
                if (moves?.Count > 0)
                {
                    return [.. moves.Cast<T>()];
                }

            }

            // If no moves are available, return an empty list of type T
            return [];
        }

        #endregion

        #region -- List Concatenation Methods --

        /// <summary>
        /// Concatenates two lists of ChessSquareLocation and returns a new list containing all elements from both lists.
        /// </summary>
        /// <param name="list1"> a list of chessSquare locations </param>
        /// <param name="list2"> another list of chessSquare locations </param>
        /// <returns> a new list containing all elements from both lists </returns>
        public static List<ChessSquareLocation> ConcatList(List<ChessSquareLocation> list1, List<ChessSquareLocation> list2)
        {
            return [.. Enumerable.Concat(list1, list2)];
        }

        /// <summary>
        /// Concatenates two lists of MovesAvailable and returns a new list containing all elements from both lists.
        /// </summary>
        /// <param name="list1"> a list of moves available </param>
        /// <param name="list2"> another list of moves available </param>
        /// <returns> a new list containing all elements from both lists </returns>
        public static List<MovesAvailable>? ConcatList(List<MovesAvailable> list1, List<MovesAvailable> list2)
        {
            return [.. Enumerable.Concat(list1, list2)];
        }

        /// <summary>
        /// Concatenates two lists of Peek objects and returns a new list containing all elements from both lists.
        /// </summary>
        /// <param name="list1"> a list of peek objects </param>
        /// <param name="list2"> another list of peek objects </param>
        /// <returns> a new list containing all elements from both lists </returns>
        private static List<Peek> ConcatList(List<Peek> list1, List<Peek> list2)
        {
            return [.. Enumerable.Concat(list1, list2)];
        }

        #endregion

        #region -- Chess Piece Cloning Methods --
        /// <summary>
        /// Creates a deep clone of the given chess piece based on its type. 
        /// It uses the specific clone method for each piece type (Pawn, Knight, Rook, Bishop, Queen, King) to create a new instance with the same properties.
        /// </summary>
        /// <param name="chessPiece"> The chess piece to clone </param>
        /// <returns> A new instance of the same type with the same properties </returns>
        public static ChessPiece? CloneChessPieceOnType(ChessPiece chessPiece)
        {
            try
            {
                switch (chessPiece.PieceType)
                {
                    case PieceType.PAWN:
                        Pawn pawn = (Pawn)chessPiece;
                        return pawn.DeepClone_();
                    case PieceType.KNIGHT:
                        Knight knight = (Knight)chessPiece;
                        return knight.DeepClone_();
                    case PieceType.ROOK:
                        Rook rook = (Rook)chessPiece;
                        return rook.DeepClone_();
                    case PieceType.BISHOP:
                        Bishop bishop = (Bishop)chessPiece;
                        return bishop.DeepClone_();
                    case PieceType.QUEEN:
                        Queen queen = (Queen)chessPiece;
                        return queen.DeepClone_();
                    case PieceType.KING:
                        King king = (King)chessPiece;
                        return king.DeepClone_();
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ERORR!!! {e.Message}");
            }

            return null;
        }

        #endregion
    }
}
