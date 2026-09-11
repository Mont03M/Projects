using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.PeekMoves;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Structs;
using ChessGame.Utilities;

namespace ChessGame.ChessPieces
{
    /// <summary>
    /// Represents a Knight chess piece, inheriting from the ChessPiece class and implementing the IChessMoves interface.
    /// </summary>
    /// <param name="chessGameService"> an object that contains the chess game service </param>
    /// <param name="pieceType"> the type of the chess piece </param>
    /// <param name="pieceColor"> the color of the chess piece </param>
    /// <param name="currentLocation"> the current location of the chess piece </param>
    /// <param name="pieceImage"> the image representing the chess piece </param>
    public class Knight(IIChessGameService chessGameService, PieceType pieceType,
      ChessPieceColors pieceColor, ChessSquareLocation currentLocation, string pieceImage) : 
        ChessPiece(chessGameService, pieceType, pieceColor, currentLocation, pieceImage), IChessMoves
    {
        #region -- Properties --
        public List<MovesAvailable>? AvailableMoves { get; set; } = [];
        public List<MovesAvailable>? AttackMoves { get; set; } = [];
        private List<Peek>? _allAvailableMoves = [];

        public List<Peek>? AllAvailableMoves
        {
            get
            {
                lock (SyncRoot.MovesLock)
                {
                    return _allAvailableMoves;
                }
            }
            set
            {
                lock (SyncRoot.MovesLock)
                {
                    _allAvailableMoves = value;
                }
            }
        }

        #endregion

        #region -- Main Methods --
        /// <summary>
        /// Generates valid moves for the Knight piece based on the current state of the chessboard, taking into account pinning and allowed pin moves.
        /// </summary>
        /// <param name="chessBoard">The chessboard containing the current game state.</param>
        public void GenerateValidMoves(Board chessBoard)
        {
            // First detect pin state so generators can respect AllowedPinMoves
            var pinned = IsChessPiecePinned(chessBoard);

            // Now (re)generate moves; generators will consult AllowedPinMoves when building lists
            GenerateAvailableMoves(chessBoard);
            GenerateAttackMoves(chessBoard);

            if (pinned)
            {
                // If the piece is pinned, filter the available and attack moves to only include those that are allowed by the pin
                if (this.AllowedPinMoves != null)
                {
                    // Defensive: ensure AllowedPinMoves is not null before filtering
                    lock (SyncRoot.MovesLock) 
                    {
                        if (AvailableMoves != null)
                            AvailableMoves = [.. AvailableMoves.Where(mv => this.AllowedPinMoves.Any(a => a.Equals(mv.Move)))];

                        if (AttackMoves != null)
                            AttackMoves = [.. AttackMoves.Where(mv => this.AllowedPinMoves.Any(a => a.Equals(mv.Move)))];
                    }
                }
            }

        }

        /// <summary>
        /// Generates all available moves for the Knight piece, considering the current state of the chessboard and any pinning restrictions. This method populates the AvailableMoves
        /// list with valid moves that the Knight can make, ensuring that moves are within bounds and do not conflict with pieces of the same color.
        /// </summary>
        /// <param name="chessBoard">The chessboard containing the current game state.</param>
        public void GenerateAvailableMoves(Board chessBoard)
        {
            if(chessBoard.ChessSquares_ == null) return;

            if (AvailableMoves?.Count > 0)
            {
                // Clear the list of available moves in a thread-safe manner
                lock (SyncRoot.MovesLock)
                {
                    AvailableMoves.Clear();
                }
            }

            // Clear backing list in a thread-safe manner
            ReplaceAllAvailableMoves(null);

            if (!this.IsPinned)
            {
                this.AllowedPinMoves = null;
            }

            // Generate all possible knight moves from the current location
            var knightMoves = this.KnightMoves([]);

            // Get the list of chess pieces for the current player based on the piece color
            var playerChessPieces = ((this.PieceColor == ChessPieceColors.WHITE)
              ? chessBoard.WhiteChessPieces
              : chessBoard.BlackChessPieces);

            foreach (var move in knightMoves)
            {
                // Check if the move is within the bounds of the chessboard and does not conflict with pieces of the same color
                if (move.IsInBounds() && !this.IsSamePieceColor(move, playerChessPieces))
                {
                    // If the piece is pinned, only add moves that are allowed by the pin; otherwise, add all valid moves
                    if (this.IsPinned && this.AllowedPinMoves != null)
                    {
                        // Defensive: ensure AllowedPinMoves is not null before checking for allowed moves
                        if (this.AllowedPinMoves.Any(a => a.Equals(move)))
                           lock (SyncRoot.MovesLock)
                           {
                               AvailableMoves?.Add(new MovesAvailable(MoveType.NORMAL, this.PieceType, move, this));
                           }
                    }
                    else
                    {
                        // If not pinned, add all valid moves
                        lock (SyncRoot.MovesLock)
                        {
                            AvailableMoves?.Add(new MovesAvailable(MoveType.NORMAL, this.PieceType, move, this));
                        }
                    }
                }
            }

            // Generate peek moves for the Knight piece on the given chess board
            GeneratePeekMoves();
        }

        /// <summary>
        /// Generates attack moves for the Knight piece on the given chess board, considering available moves and opponent positions.
        /// </summary>
        /// <param name="chessBoard">The chess board on which to generate attack moves.</param>
        public void GenerateAttackMoves(Board chessBoard)
        {
            if (chessBoard?.ChessSquares_ == null) return;
            
            if(AttackMoves?.Count > 0)
            {
                // Clear the AttackMoves list before generating new attack moves
                lock (SyncRoot.MovesLock)
                {
                    AttackMoves?.Clear();
                }
            }

            // Find the opponent's king on the chess board
            var otherKing = ChessPieceExtenstions.FindChessPiece(
                PieceType.KING,
                ChessPieceExtenstions.GetOtherColor(this.PieceColor),
                chessBoard.ChessSquares_) as King;

            var opponentPositions = ((this.PieceColor == ChessPieceColors.WHITE)
                ? chessBoard.BlackChessPieces
                : chessBoard.WhiteChessPieces)?
                .Select(p => p.CurrentLocation)
                .ToHashSet() ?? [];

            // Iterate a snapshot to be safe if collections change elsewhere
            foreach (var move in AvailableMoves?.ToList() ?? [])
            {
                if (move.Move == otherKing?.CurrentLocation)
                {
                    move.MoveType = MoveType.CHECK;
                    lock (SyncRoot.MovesLock) // Ensure thread safety when adding to AttackMoves
                    {
                        AttackMoves?.Add(move);
                    }
                    continue;
                }

                // If the move is in the opponent's positions, mark it as a capture move and add it to AttackMoves
                if (opponentPositions.Contains(move.Move))
                {
                    move.MoveType = MoveType.CAPTURE;
                    lock (SyncRoot.MovesLock) // Ensure thread safety when adding to AttackMoves
                    {
                        AttackMoves?.Add(move);
                    }
                }
            }
        }

        /// <summary>
        /// Generates peek moves for the Knight piece on the given chess board, considering L moves and available peek positions.
        /// </summary>
        /// <param name="chessBoard">The chess board on which to generate peek moves.</param>
        public void GeneratePeekMoves()
        {
            // Clear the existing peek moves before generating new ones
            var peekMoves = this.KnightMoves([]);

            foreach (var peek in peekMoves)
            {
                if (peek.IsInBounds())
                    AddAvailablePeek(new Peek(peek, this.PieceType, this.PieceColor));
            }

            // Defensive: ensure no null Peek entries remain in the backing list
            // (some code paths elsewhere may mistakenly insert nulls)
            ReplaceAllAvailableMoves(GetAllAvailableMovesSnapshot().Where(p => p != null));
        }


        /// <summary>
        /// Generates check moves for the Knight piece on the given chess board, considering the attacker and path to the king.
        /// </summary>
        /// <param name="attacker">The attacking piece.</param>
        /// <param name="pathToKing">The path from the attacker to the king.</param>
        /// <param name="chessBoard">The chess board on which to generate check moves.</param>
        public void GenerateIsCheckMoves(ChessPiece attacker, List<ChessSquareLocation>? pathToKing, Board? chessBoard = null)
        {
            if (attacker is null) return;

            // If pinned, pawn can't block or attack along the line
            if (this.IsPinned)
            {
                AvailableMoves?.Clear();
                AttackMoves?.Clear();
                return;
            }

            bool attackerIsShortRange = attacker is Knight || attacker is Pawn;
            var attackerTarget = attacker.CurrentLocation;

            // If attacker is a knight/pawn and there is no line-of-check, the only valid attack move is the attacker's square.
            if (attackerIsShortRange && pathToKing is null)
            {
                if (AttackMoves != null && chessBoard != null)
                    AttackMoves.RemoveAll(m => !m.Move.Equals(attackerTarget));

                AvailableMoves?.Clear();
                return;
            }

            // If there is a path-to-king, restrict available moves to that path
            if (pathToKing != null)
            {
                var pathSet = new HashSet<ChessSquareLocation>(pathToKing);

                AvailableMoves?.RemoveAll(m => !pathSet.Contains(m.Move));

                AttackMoves?.RemoveAll(m => !m.Move.Equals(attackerTarget));
            }
            else
            {
                // No pathToKing & attacker is not short-range:
                // keep only attack/special moves that directly capture/check the attacker square
                AttackMoves?.RemoveAll(m => !m.Move.Equals(attackerTarget));
            }
        }

        #endregion

        #region -- Thread-Safe Move Management -- 

        /// <summary>
        /// Adds a new Peek move to the list of all available moves for the Knight piece, ensuring thread safety and avoiding null entries.
        /// </summary>
        /// <param name="p">The Peek move to add.</param>
        public void AddAvailablePeek(Peek? p)
        {
            if (p == null) return;
            lock (SyncRoot.MovesLock)
            {
                _allAvailableMoves ??= [];
                _allAvailableMoves.Add(p);
            }
        }

        /// <summary>
        /// Replaces the entire list of all available moves for the Knight piece with a new collection, ensuring thread safety and avoiding null entries.
        /// </summary>
        /// <param name="items">The new collection of peek moves.</param>
        public void ReplaceAllAvailableMoves(IEnumerable<Peek>? items)
        {
            lock (SyncRoot.MovesLock)
            {
                _allAvailableMoves = items?.Where(x => x != null).ToList() ?? [];
            }
        }

        /// <summary>
        /// Gets a snapshot of all available moves for the Knight piece, ensuring thread safety and returning a copy of the list to avoid external modifications.
        /// </summary>
        /// <returns>A list of all available peek moves.</returns>
        public List<Peek> GetAllAvailableMovesSnapshot()
        {
            lock (SyncRoot.MovesLock)
            {
                return _allAvailableMoves == null ? [] : [.. _allAvailableMoves];
            }
        }

        #endregion

        #region -- Deep Clone Method --
        /// <summary>
        /// Creates a deep clone of the current Knight object, including its properties and available moves, ensuring that the cloned object is independent of the original.
        /// </summary>
        /// <returns>A deep clone of the current Knight object.</returns>
        public Knight DeepClone_()
        {
            // Create a new Knight instance with the same properties as the current instance
            var clone = new Knight(this.ChessGameService, this.PieceType, this.PieceColor,
                new ChessSquareLocation(CurrentLocation.X, CurrentLocation.Y), this.PieceImage)
            {
                AvailableMoves = this.AvailableMoves != null ? [.. this.AvailableMoves] : null,
                AttackMoves = this.AttackMoves != null ? [.. this.AttackMoves] : null,
                AllAvailableMoves = this.AllAvailableMoves != null ? [.. this.AllAvailableMoves] : null,
                IsPinned = this.IsPinned
            };

            return clone;
        }
        #endregion
    }
}
