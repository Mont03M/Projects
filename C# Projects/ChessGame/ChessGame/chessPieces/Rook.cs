using ChessGame.ChessBoard;
using ChessGame.Square;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.PeekMoves;
using ChessGame.Enums;
using ChessGame.Structs;
using System.Collections.ObjectModel;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Utilities;

namespace ChessGame.ChessPieces
{
    /// <summary>
    /// Represents a Rook chess piece, inheriting from the ChessPiece class and implementing the IChessMoves interface.
    /// </summary>
    /// <param name="chessGameService"> an object that contains the chess game service </param>
    /// <param name="pieceType"> the type of the chess piece </param>
    /// <param name="pieceColor"> the color of the chess piece </param>
    /// <param name="currentLocation"> the current location of the chess piece </param>
    /// <param name="pieceImage"> the image representing the chess piece </param>
    public class Rook(IIChessGameService chessGameService, PieceType pieceType,
      ChessPieceColors pieceColor, Special_Moves castling, ChessSquareLocation currentLocation, string pieceImage, CastlingSide castleSide) 
        : ChessPiece(chessGameService, pieceType, pieceColor, currentLocation, pieceImage), 
        IChessMoves, ICastlingMove, IPieceSpecialMove, IMoved
    {
        #region -- Properties --
        public ChessSquareLocation StartLocation { get; set; } = currentLocation;
        private bool HasMoved_ { get; set; } = false;
        public List<MovesAvailable>? AvailableMoves { get; set; } = [];
        public List<MovesAvailable>? AttackMoves { get; set; } = [];
        public List<MovesAvailable>? SpecialMoves { get; set; } = [];
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

        public CastlingSide CastleSide { get; set; } = castleSide;
        // special moves
        public Special_Moves Castling { get; set; } = castling;

        public bool ChessPieceMoved
        {
            get => HasMoved_;
            set
            {
                HasMoved_ = value;
                OnPropertyChanged(nameof(HasMoved_));
            }
        }

        #endregion

        #region -- Main Methods -- 

        /// <summary>
        /// Generates valid moves for the Rook piece on the given chess board, taking into account pin states and allowed pin moves.
        /// </summary>
        /// <param name="chessBoard"> the chess board on which to generate moves </param>
        public void GenerateValidMoves(Board chessBoard)
        {
            // First detect pin state so generators can respect AllowedPinMoves
            var pinned = IsChessPiecePinned(chessBoard);

            // Now (re)generate moves; generators will consult AllowedPinMoves when building lists
            GenerateAvailableMoves(chessBoard);
            GenerateAttackMoves(chessBoard);

            // If pinned, filter already-generated moves to ensure consistency
            if (pinned)
            {
                // If the piece is pinned, filter available and attack moves to only those allowed by the pin
                if (this.AllowedPinMoves != null)
                {
                    lock (SyncRoot.MovesLock)
                    {
                        // Filter AvailableMoves and AttackMoves to only those that are in AllowedPinMoves
                        if (AvailableMoves != null)
                            AvailableMoves = [.. AvailableMoves.Where(mv => this.AllowedPinMoves.Any(a => a.Equals(mv.Move)))];

                        if (AttackMoves != null)
                            AttackMoves = [.. AttackMoves.Where(mv => this.AllowedPinMoves.Any(a => a.Equals(mv.Move)))];


                        if (SpecialMoves != null)
                            SpecialMoves = [.. SpecialMoves.Where(mv => this.AllowedPinMoves.Any(a => a.Equals(mv.Move)))];
                    }
                }
            }
        }

        /// <summary>
        /// Generates available moves for the Rook piece on the given chess board, considering pin states and allowed pin moves.
        /// </summary>
        /// <param name="chessBoard"> the chess board on which to generate moves </param>
        public void GenerateAvailableMoves(Board chessBoard)
        {
            if(chessBoard.ChessSquares_ == null) return;

            if (AvailableMoves?.Count > 0)
            {
                lock (SyncRoot.MovesLock)
                {
                    AvailableMoves.Clear();
                }
            }

            // Defensive: ensure no null Peek entries remain in the backing list
            ReplaceAllAvailableMoves(null);

            if (!this.IsPinned)
            {
                this.AllowedPinMoves = null;
            }

            Moved();

            // get horizontal and vertical x and y locations
            var rookMoves = HorizontalAndVerticalMoves(this.CurrentLocation, chessBoard.ChessSquares_);

            // get the list of chess pieces for the current player based on the piece color
            var playerChessPieces = ((this.PieceColor == ChessPieceColors.WHITE)
              ? chessBoard.WhiteChessPieces
              : chessBoard.BlackChessPieces);

            // loop through list
            foreach (var move in rookMoves)
            {
                // check if the move is within bounds and not occupied by a piece of the same color
                if (move.IsInBounds() && !this.IsSamePieceColor(move, playerChessPieces))
                {
                    // If the piece is pinned, only add moves that are in AllowedPinMoves; otherwise, add all valid moves
                    if (this.IsPinned && AllowedPinMoves != null)
                    {
                        // Only add moves that are in AllowedPinMoves
                        if (this.AllowedPinMoves.Any(a => a.Equals(move)))
                            lock (SyncRoot.MovesLock) // Ensure thread safety when adding to AvailableMoves
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

            GeneratePeekMoves(chessBoard.ChessSquares_);
        }

        /// <summary>
        /// Generates attack moves for the Rook piece on the given chess board, considering available moves and opponent positions.
        /// </summary>
        /// <param name="chessBoard">The chess board on which to generate attack moves.</param>
        public void GenerateAttackMoves(Board chessBoard)
        {
            if (chessBoard?.ChessSquares_ == null) return;

            // Defensive: if there are no available moves, clear the attack moves and return early
            if (AvailableMoves == null || AvailableMoves.Count == 0)
            {
                AttackMoves?.Clear();
                return;
            }

            // Clear the AttackMoves list before generating new attack moves
            lock (SyncRoot.MovesLock)
            {
                AttackMoves?.Clear();
            }

            // Find the opponent's king on the chess board
            var otherKing = ChessPieceExtenstions.FindChessPiece(
                PieceType.KING,
                ChessPieceExtenstions.GetOtherColor(this.PieceColor),
                chessBoard.ChessSquares_) as King;

            // Get the positions of the opponent's pieces based on the current piece's color
            var opponentPositions = ((this.PieceColor == ChessPieceColors.WHITE)
                ? chessBoard.BlackChessPieces
                : chessBoard.WhiteChessPieces)?
                .Select(p => p.CurrentLocation)
                .ToHashSet() ?? [];

            // Iterate a snapshot to be safe if collections change elsewhere
            foreach (var move in AvailableMoves.ToList())
            {
                if (move.Move.Equals(otherKing?.CurrentLocation))
                {
                    move.MoveType = MoveType.CHECK;
                    lock (SyncRoot.MovesLock) // Ensure thread safety when adding to AttackMoves
                    {
                        AttackMoves?.Add(move);
                    }
                    continue;
                }

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
        /// Generates check moves for the Rook piece on the given chess board, considering the attacker and path to the king.
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

            // If the attacker is a knight or pawn, it can only be captured on its square (no blocking possible)
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

                SpecialMoves?.Clear();
            }
            else
            {
                // No pathToKing & attacker is not short-range:
                // keep only attack/special moves that directly capture/check the attacker square
                AttackMoves?.RemoveAll(m => !m.Move.Equals(attackerTarget));

                SpecialMoves?.Clear();
            }
        }

        /// <summary>
        /// Generates peek moves for the Rook piece on the given chess board, considering horizontal and vertical moves and available peek positions.
        /// </summary>
        /// <param name="chessBoard">The chess board on which to generate peek moves.</param>
        public void GeneratePeekMoves(ObservableCollection<ChessSquare> chessBoard)
        {
            var peekMoves = HorizontalAndVerticalMoves(this.CurrentLocation, chessBoard, generateAllMoves: true);

            foreach (var peek in peekMoves)
            {
                if (peek.IsInBounds())
                    AddAvailablePeek(new Peek(peek, this.PieceType, this.PieceColor));
            }

            // Defensive: ensure no null Peek entries remain in the backing list
            // (some code paths elsewhere may mistakenly insert nulls)
            ReplaceAllAvailableMoves(GetAllAvailableMovesSnapshot().Where(p => p != null));
        }

        #endregion

        #region -- Castling Moves Generation Method --

        /// <summary>
        /// This method does not implement any functionality.
        /// </summary>
        /// <param name="chessBoard"> Chess Squares </param>
        /// <param name="king"> Chess Squares </param>
        /// <param name="rookQS"> a rook chess piece on the queen side</param>
        /// <param name="rookKS"> a rook chess piece on the king side</param>
        /// <param name="SquaresAttacked"> Guard chess squares and rook locations</param>
        /// <exception cref="NotImplementedException"></exception>
        public void GenerateCasltingMove(Board chessBoard, King? king, Rook? rookQS, Rook? rookKS, 
            List<(ChessSquareLocation AttackedSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked)
        {
           throw new NotImplementedException();
        }

        #endregion

        #region -- Helpers ---

        /// <summary>
        /// Marks the Rook as having moved if its current location is different from its starting location.
        /// </summary>
        public void Moved()
        {
            // has rooked moved
            if (!StartLocation.Equals(this.CurrentLocation.X, this.CurrentLocation.Y))
            {
                ChessPieceMoved = true;
            }
        }

        #endregion 

        #region -- Thread-Safe Move Management -- 

        /// <summary>
        /// Adds a new Peek move to the list of all available moves for the Rook piece, ensuring thread safety and avoiding null entries.
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
        /// Replaces the entire list of all available moves for the Rook piece with a new collection, ensuring thread safety and avoiding null entries.
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
        /// Gets a snapshot of all available moves for the Rook piece, ensuring thread safety and returning a copy of the list to avoid external modifications.
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
        /// Creates a deep clone of the Rook piece, including its properties and available moves, ensuring that the clone is independent of the original instance.
        /// </summary>
        /// <returns>A deep clone of the Rook piece.</returns>
        public Rook DeepClone_()
        {
            // Create a new rook instance with the same properties as the current instance
            var clone = new Rook(this.ChessGameService, this.PieceType, this.PieceColor, Castling,
                new ChessSquareLocation(CurrentLocation.X, CurrentLocation.Y), this.PieceImage, CastleSide)
            {
                StartLocation = this.StartLocation,
                Castling = this.Castling,
                CastleSide = this.CastleSide,
                ChessPieceMoved = this.ChessPieceMoved,
                AvailableMoves = this.AvailableMoves != null ? [.. this.AvailableMoves] : null,
                SpecialMoves = this.SpecialMoves != null ? [.. this.SpecialMoves] : null,
                AttackMoves = this.AttackMoves != null ? [.. this.AttackMoves] : null,
                AllAvailableMoves = this.AllAvailableMoves != null ? [.. this.AllAvailableMoves] : null,
                IsPinned = this.IsPinned
            };

            return clone;
        }

        #endregion
    }
}
