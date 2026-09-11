using ChessGame.ChessBoard;
using ChessGame.Square;
using ChessGame.Controls;
using ChessGame.Enums;
using ChessGame.Structs;
using ChessGame.Utilities;
using ChessGame.Comparer;
using System.Collections.ObjectModel;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.ChessPieces.Extenstions;

namespace ChessGame.ChessPieces
{
    /// <summary>
    /// Represents a chess piece in the game, encapsulating its type, color, current location, and associated behaviors such as move generation and pin detection.
    /// </summary>
    public class ChessPiece : ObservableObject
    {
        #region -- Properties ---
        public IIChessGameService ChessGameService { get; set; } = default!;
        public PieceType PieceType { get; set; } = default;
        public ChessPieceColors PieceColor { get; set;} = default!;
        public string PieceImage { get; set; } = default!;

        private ChessSquareLocation currentLocation;

        private bool IsPinnedChessPiece { get; set; }

        private List<ChessSquareLocation>? AllowedPinMoves_ { get; set; } = null;

        public ChessSquareLocation CurrentLocation
        {
            get => currentLocation;
            set
            {
                currentLocation = value;
                OnPropertyChanged(nameof(CurrentLocation));
            }
        }

        // indicates whether this piece is currently pinned (blocks attack to its king)
        public bool IsPinned
        {
            get => IsPinnedChessPiece;
            set
            {
                IsPinnedChessPiece = value;
                OnPropertyChanged(nameof(IsPinned));
            }
        }

        // When pinned, allowed moves (squares) that keep the king safe
        public List<ChessSquareLocation>? AllowedPinMoves
        {
            get => AllowedPinMoves_;

            set
            {
                AllowedPinMoves_ = value;
                OnPropertyChanged(nameof(AllowedPinMoves));
            }
        }

        #endregion

        #region -- Constructors 1 --
        public ChessPiece(IIChessGameService chessGameService, PieceType pieceType,
           ChessPieceColors pieceColor, ChessSquareLocation currentLocation, string pieceImage)
        {
            this.ChessGameService = chessGameService;
            this.PieceType = pieceType;
            this.CurrentLocation = currentLocation;
            this.PieceImage = pieceImage;
            this.PieceColor = pieceColor;
        }

        #endregion

        #region -- Constructors 2 --
        // constructor 2
        public ChessPiece(ChessPieceColors pieceColor, ChessSquareLocation currentLocation)
        {
            this.PieceColor = pieceColor;
            CurrentLocation = currentLocation;
        }

        #endregion

        #region -- Constructors 3 --
        // constructor 3
        public ChessPiece(PieceType pieceType,
            ChessPieceColors pieceColor, ChessSquareLocation currentLocation)
        {
            this.PieceType = pieceType;
            this.PieceColor = pieceColor;
            this.currentLocation = currentLocation;
        }
        #endregion

        #region -- Constructors 4 --
        // constructor 4
        public ChessPiece(PieceType pieceType, string pieceImage, Pawn pawn, ChessSquareLocation location) 
        {
            this.PieceType = pieceType;
            this.PieceImage = pieceImage;
            this.PieceColor = pawn.PieceColor;
            this.CurrentLocation = location;
            this.ChessGameService = pawn.ChessGameService;
        }

        #endregion

        #region -- Pin Detection Method --
        /// <summary>
        /// Returns true when this piece is pinned (exposes king if moved). Also sets IsPinned property.
        /// </summary>
        /// <param name="chessBoard"> the chessboard</param>
        /// <returns> true if the piece is pinned, false otherwise</returns>
        public bool IsChessPiecePinned(Board chessBoard)
        {
            IsPinned = false;

            // copy the board to avoid mutating the original during pin detection
            var boardCopy = chessBoard.DeepClone();

            // guard against null chessboard or missing squares
            if (boardCopy.ChessSquares_ == null)
            {
                IsPinned = false;
                return false;
            }

            // find this chess piece's king

            // guard against missing king (avoid NullReferenceException)
            if (ChessPieceExtenstions.FindChessPiece(PieceType.KING, this.PieceColor, boardCopy.ChessSquares_) is not King king)
                return false;

            // collect allowed moves across all potential pinning attackers
            List<ChessSquareLocation>? combinedAllowed = null;

            foreach (var square in boardCopy.ChessSquares_)
            {
                if (square.ChessPiece_ is ChessPiece attacker
                    && attacker.PieceColor != this.PieceColor
                    && this is IChessMoves pinChessPiece
                    && attacker is IChessMoves attackerChessMoves)
                {
                    // skip if attacker is pinned (cannot pin another piece)
                    if (attacker.IsPinned)
                        continue;

                    // ensure attacker move lists are up to date
                    attackerChessMoves.GenerateAvailableMoves(boardCopy);
                    attackerChessMoves.GenerateAttackMoves(boardCopy);

                    // find path from attacker to king (if any)
                    var pathToKing = GameControls.GetPathToKing(king, attacker);

                    if (pathToKing is null) continue;

                    // only sliding attackers (rook/bishop/queen) can pin along a path
                    if (attacker.PieceType == PieceType.PAWN || 
                        attacker.PieceType == PieceType.KNIGHT ||
                        attacker.PieceType == PieceType.KING)
                        continue;

                    // the piece to be pinned must lie on the path between attacker and king
                    if (!pathToKing.Any(p => p.Equals(this.CurrentLocation)))
                        continue;

                    // Use attackMoves when detecting pin projection — attackMoves represent the squares
                    // the attacker projects along regardless of intermediate blockers or special move filtering.
                    if ((attackerChessMoves.AttackMoves?.Count > 0 && attackerChessMoves.AttackMoves.Any(m => m.Move.Equals(this.CurrentLocation))
                        || (attackerChessMoves.AvailableMoves?.Count > 0 && attackerChessMoves.AvailableMoves.Any(m => m.Move.Equals(this.CurrentLocation))))
                        && pathToKing.Any(m => m.Equals(king.CurrentLocation)))
                    {
                        
                        // ensure attacker actually projects along the path
                        if (!(attackerChessMoves.AttackMoves?.Any(m => pathToKing.Contains(m.Move)) == true
                              || attackerChessMoves.AvailableMoves?.Any(m => pathToKing.Contains(m.Move)) == true))
                            continue;

                        // count occupied squares along the path (excluding attacker and king)
                        var occupiedOnPath = new List<ChessSquareLocation>();
                        foreach (var loc in pathToKing)
                        {
                            var sq = loc.GetSquare(boardCopy.ChessSquares_);
                            if (sq?.ChessPiece_ != null)
                            {
                                // ignore the king and the attacker squares when counting
                                if (sq.ChessPiece_.CurrentLocation.Equals(attacker.CurrentLocation.X, attacker.CurrentLocation.Y) ||
                                    sq.ChessPiece_.CurrentLocation.Equals(king.CurrentLocation.X, king.CurrentLocation.Y))
                                    continue;

                                occupiedOnPath.Add(loc);
                            }
                        }

                        // if more than one piece blocks the path, moving a single piece won't expose the king
                        if (occupiedOnPath.Count > 1)
                            continue;

                        // if there is exactly one blocker and it is not this piece, skip
                        if (occupiedOnPath.Count == 1 && !occupiedOnPath[0].Equals(this.CurrentLocation))
                            continue;

                        // Now this piece is the sole blocker (pinned) for this attacker. Compute allowed moves for this attacker.
                        var allowedForAttacker = new List<ChessSquareLocation>();

                        // squares on path (excluding the king square) are valid blocking squares
                        foreach (var loc in pathToKing)
                        {
                            if (!loc.Equals(king.CurrentLocation))
                                allowedForAttacker.Add(loc);
                        }

                        // capturing the attacker is allowed
                        allowedForAttacker.Add(attacker.CurrentLocation);

                        // combine allowed sets across multiple attackers (intersection)
                        if (combinedAllowed == null)
                            combinedAllowed = [.. allowedForAttacker];
                        else
                            combinedAllowed = [.. combinedAllowed.Intersect(allowedForAttacker, new ChessSquareLocationEqualityComparer())];
                 
                    }
                }
            }

            // if combinedAllowed was computed, apply restrictions
            if (combinedAllowed == null || combinedAllowed.Count == 0)
            {
                IsPinned = false;
                AllowedPinMoves = null;
                return false;
            }

            // mark pinned and store allowed moves for generators to consume
            IsPinned = true;
            AllowedPinMoves = combinedAllowed;

            // Do not mutate generator lists here - let piece-specific generators (Pawn, Knight, etc.) filter
            // their availableMoves/attackMoves using AllowedPinMoves. This avoids duplicate filtering and
            // ensures special move rules (en-passant, promotion) can be handled correctly per piece.
            return true;
        }

        #endregion

        #region -- Move Generation Methods --

        /// <summary>
        /// Generates the possible moves for a king piece based on its current location, adding them to the provided list of moves.
        /// </summary>
        /// <param name="moves"> the list of moves to add to</param>
        /// <returns> the updated list of moves</returns>
        public List<ChessSquareLocation> KingMoves(List<ChessSquareLocation> moves)
        {
            // offset x and y values for king's possible moves
            int[] xMoves = [-1, -1, 0, 1, 1, 1, 0, -1];

            int[] yMoves = [0, 1, 1, 1, 0, -1, -1, -1];

            for (int i = 0; i < 8; i++)
            {
                // create x and y locations with offset values
                ChessSquareLocation move = new(this.CurrentLocation.X + xMoves[i],
                    this.CurrentLocation.Y + yMoves[i]);

                moves.Add(move);
            }

            return moves;
        }

        /// <summary>
        /// Generates the possible moves for a knight piece based on its current location, adding them to the provided list of moves.
        /// </summary>
        /// <param name="moves"> the list of moves to add to</param>
        /// <returns> the updated list of moves</returns>
        public List<ChessSquareLocation> KnightMoves(List<ChessSquareLocation> moves)
        {
            // offset x and y values for knight's possible moves
            int[] xMoves = [2, 1, -1, -2, -2, -1, 1, 2];

            int[] yMoves = [1, 2, 2, 1, -1, -2, -2, -1];


            for (int i = 0; i < 8; i++)
            {
                // create x and y locations with offset values
                ChessSquareLocation move = new(CurrentLocation.X + xMoves[i],
                    CurrentLocation.Y + yMoves[i]);

                moves.Add(move);
            }

            return moves;
        }

        /// <summary>
        /// Generates the diagonal moves for a chess piece from its origin location, 
        /// considering the current state of the chessboard and whether to generate all moves or only valid ones.
        /// </summary>
        /// <param name="origin"> The origin location from which to generate diagonal moves</param>
        /// <param name="chessBoard"> The chessboard containing the current game state</param>
        /// <param name="generateAllMoves"> Whether to generate all possible moves or only valid ones</param>
        /// <returns> The list of diagonal moves</returns>
        public List<ChessSquareLocation> DiagonalMoves(ChessSquareLocation origin,
            ObservableCollection<ChessSquare> chessBoard, bool generateAllMoves = false)
        {
            var diagonals = new List<ChessSquareLocation>();

            // four diagonal directions
            var dirs = new (int dx, int dy)[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };

            // Helper to get square at coords
            ChessSquare? GetSquare(int x, int y) => chessBoard.FirstOrDefault(s => s.BoardLocation.Equals(x, y));

            foreach (var (dx, dy) in dirs)
            {
                // start from the origin and move in the diagonal direction until we hit the edge of the board or an occupied square
                int x = origin.X + dx;
                int y = origin.Y + dy;

                while (x >= 0 && x < 8 && y >= 0 && y < 8)
                {
                    var loc = new ChessSquareLocation(x, y);

                    var sq = GetSquare(x, y);

                    if (generateAllMoves)
                    {
                        // when generating all moves: include occupied squares appropriately
                        if (sq?.ChessPiece_ != null)
                        {
                            diagonals.Add(loc);

                            // if the square has a piece, we stop further diagonal moves in this direction
                            if (sq.ChessPiece_.PieceType != PieceType.KING)
                                break;

                            // if the square has a king of the same color, we also stop further diagonal moves in this direction
                            if (sq.ChessPiece_.PieceType is PieceType.KING && sq.ChessPiece_.PieceColor == this.PieceColor)
                                break;
                        }
                        else
                        {

                            diagonals.Add(loc); // empty square, continue
                        }
                    }
                    else
                    {
                        // original behaviour: treat IsMove (empty) as movable; if occupied add final square then stop
                        if (loc.IsMove(chessBoard))
                        {
                            diagonals.Add(loc);
                        }
                        else
                        {
                            diagonals.Add(loc);
                            break;
                        }
                    }

                    // move to the next square in the diagonal direction
                    x += dx;
                    y += dy;
                }
            }

            return diagonals;
        }

        /// <summary>
        /// Finds the horizontal and vertical moves for a chess piece from its origin location.
        /// </summary>
        /// <param name="origin"> The origin location from which to generate moves</param>
        /// <param name="chessBoard"> The chessboard containing the current game state</param>
        /// <param name="generateAllMoves"> Whether to generate all possible moves or only valid ones</param>
        /// <returns> The list of horizontal and vertical moves</returns>
        public List<ChessSquareLocation> HorizontalAndVerticalMoves(ChessSquareLocation origin,
            ObservableCollection<ChessSquare> chessBoard, bool generateAllMoves = false)
        {
            var moves = new List<ChessSquareLocation>();

            // four straight directions: up, down, left, right
            var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };

            ChessSquare? GetSquare(int x, int y) => chessBoard.FirstOrDefault(s => s.BoardLocation.Equals(x, y));

            foreach (var (dx, dy) in dirs)
            {
                int x = origin.X + dx;
                int y = origin.Y + dy;

                while (x >= 0 && x < 8 && y >= 0 && y < 8)
                {
                    // create a new ChessSquareLocation for the current coordinates
                    var loc = new ChessSquareLocation(x, y);
                    var sq = GetSquare(x, y);

                    // If generating all moves, we include occupied squares appropriately
                    if (generateAllMoves)
                    {
                        // when generating all moves: include occupied squares appropriately
                        if (sq?.ChessPiece_ != null)
                        {
                            moves.Add(loc);

                            // if the square has a piece, we stop further moves in this direction
                            if (sq.ChessPiece_.PieceType != PieceType.KING)
                                break;

                            // if the square has a king of the same color, we also stop further moves in this direction
                            if (sq.ChessPiece_.PieceType is PieceType.KING && sq.ChessPiece_.PieceColor == this.PieceColor)
                                break;
                        }
                        else
                        {

                            moves.Add(loc); // empty square, continue
                        }
                    }
                    else
                    {
                        // empty square: include and continue; occupied square: include and stop
                        if (loc.IsMove(chessBoard))
                        {
                            moves.Add(loc);
                        }
                        else
                        {
                            moves.Add(loc);
                            break;
                        }
                    }

                    // move to the next square in the current direction
                    x += dx;
                    y += dy;
                }
            }

            return moves;
        }

# endregion

        #region -- Helper Methods --
        /// <summary>
        /// Checks if the specified move corresponds to a chess piece of the same color in the provided list of player chess pieces.
        /// </summary>
        /// <param name="move"> a chessSquareLocation of (x,y) coordinates</param>
        /// <param name="playerChessPieces"> a list of player's chess pieces</param>
        /// <returns> true if the move corresponds to a piece of the same color, false otherwise</returns>
        public bool IsSamePieceColor(ChessSquareLocation move, List<ChessPiece> playerChessPieces)
        {
            // loop through array
            foreach (var cp in playerChessPieces)
            {
                // check if null
                if (cp != null)
                {
                    // colors match ??
                    if (cp.CurrentLocation == move
                        && this.PieceColor == cp.PieceColor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the specified move corresponds to an empty square on the chessboard.
        /// </summary>
        /// <param name="move"> a chessSquareLocation of (x,y) coordinates</param>
        /// <param name="chessBoard"> the chessboard</param>
        /// <returns> true if the square is empty, false otherwise</returns>
        public static bool IsSquareEmpty(ChessSquareLocation move, ObservableCollection<ChessSquare> chessBoard)
        {
            foreach (var square in chessBoard)
            {
                // check if the square's location matches the move and if it is empty (no chess piece)
                if (square.BoardLocation == move && square.ChessPiece_ == null)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region -- Clone Method --

        /// <summary>
        /// Creates a deep copy of the current ChessPiece instance, including its type, color, and current location.
        /// </summary>
        /// <returns> a clone of the current ChessPiece</returns>
        public ChessPiece Clone()
        {
            return new ChessPiece(PieceType, PieceColor, 
                new ChessSquareLocation(currentLocation.X, currentLocation.Y));
        }

        #endregion

    }
}
