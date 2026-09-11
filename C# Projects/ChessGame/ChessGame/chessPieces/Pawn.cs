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
    /// Represents a Pawn chess piece, inheriting from the ChessPiece class and implementing the IChessMoves interface.
    /// </summary>
    /// <param name="chessGameService"> an object that contains the chess game service </param>
    /// <param name="pieceType"> the type of the chess piece </param>
    /// <param name="pieceColor"> the color of the chess piece </param>
    /// <param name="currentLocation"> the current location of the chess piece </param>
    /// <param name="pieceImage"> the image representing the chess piece </param>
    public class Pawn : ChessPiece, IChessMoves, IPieceSpecialMove, IMoved
    {
        #region -- Properties --
        public ChessSquareLocation StartLocation { get; set; } = default;
        private bool HasMoved_ { get; set; }
        private int PawnIndex_ { get; set; } = 0;
        public List<MovesAvailable>? AvailableMoves { get; set; } = default;
        public List<MovesAvailable>? AttackMoves { get; set; } = default;
        public List<MovesAvailable>? SpecialMoves { get; set; } = default;
        private List<Peek>? _allAvailableMoves = [];

        public List<Peek>? AllAvailableMoves
        {
            get
            {
                lock (SyncRoot.MovesLock)
                {
                    // return internal list reference so existing callers that mutate the list continue to work
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

       
        public Special_Moves[] SpecialListOfMoves { get; set; } = default!;
        private bool CanCaptureEnpassant_ { get; set; }

        public int PawnIndex
        {
            get => PawnIndex_;
            set
            {
                PawnIndex_ = value;
                OnPropertyChanged(nameof(PawnIndex));
            }
        }

        public bool ChessPieceMoved
        {
            get => HasMoved_;
            set
            {
                HasMoved_ = value;
                OnPropertyChanged(nameof(ChessPieceMoved));
            }
        }

        public bool CanCaptureEnpassant
        {
            get => CanCaptureEnpassant_;
            set
            {
                CanCaptureEnpassant_ = value;
                OnPropertyChanged(nameof(CanCaptureEnpassant));
            }
        }

        #endregion

        #region -- Constructor 1 --

        public Pawn(IIChessGameService chessGameService, PieceType pieceType, ChessPieceColors pieceColor, 
            Special_Moves[] specialMoves, ChessSquareLocation currentLocation, string pieceImage) 
            : base(chessGameService, pieceType, pieceColor, currentLocation, pieceImage)
        {
            AttackMoves = [];
            AvailableMoves = [];
            this.SpecialMoves = [];
            AllAvailableMoves = [];
            StartLocation = currentLocation;

            this.SpecialListOfMoves = specialMoves;
            ChessPieceMoved = false;
            CanCaptureEnpassant = false;
        }

        #endregion

        #region -- Constructor 2 --
        public Pawn(PieceType pieceType, ChessPieceColors pieceColor, ChessSquareLocation currentLocation, List<MovesAvailable> moves)
           : base(pieceType, pieceColor, currentLocation)
        {
            AvailableMoves = [.. moves];
        }

        #endregion

        #region -- Main Methods --
        /// <summary>
        /// Generates valid moves for the Pawn piece on the given chess board, taking into account pin states and allowed pin moves.
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
                if (this.AllowedPinMoves != null)
                {
                    // Filter AvailableMoves and AttackMoves to only those that are in AllowedPinMoves
                    lock (SyncRoot.MovesLock)
                    {
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
        /// Generates available moves for the Pawn piece on the given chess board, considering pin states and allowed pin moves.
        /// </summary>
        /// <param name="chessBoard"> the chess board on which to generate moves </param>
        public void GenerateAvailableMoves(Board chessBoard)
        {
            if (chessBoard?.ChessSquares_ == null) return;

            // clear/trim lists under shared lock to avoid concurrent mutations
            lock (SyncRoot.MovesLock)
            {
                AvailableMoves?.Clear();

                // Keep only en-passant in SpecialMoves
                if (SpecialMoves?.Count > 0)
                    SpecialMoves.RemoveAll(s => s.MoveType != MoveType.EnPASSENT);
            }

            // If not pinned, clear allowed-pin moves
            if (!this.IsPinned)
                this.AllowedPinMoves = null;

            Moved(); // preserved original call position

            var squares = chessBoard.ChessSquares_;
            bool isPlayer = this.PieceColor == ChessGameService.playerColor;
            int forward = isPlayer ? -1 : +1;
            int promotionFromRank = isPlayer ? 1 : 6;

            bool InBounds(ChessSquareLocation p) => p.X >= 0 && p.X <= 7 && p.Y >= 0 && p.Y <= 7;

            bool PinAllows(ChessSquareLocation p) =>
                !this.IsPinned || this.AllowedPinMoves == null || this.AllowedPinMoves.Any(a => a.Equals(p));

            bool CanAddForward(ChessSquareLocation p) =>
                InBounds(p) && IsSquareEmpty(p, squares) && PinAllows(p);

            void AddNormalMoveWithPromotion(ChessSquareLocation p)
            {
                if (!CanAddForward(p)) return;

                if (this.CurrentLocation.X == promotionFromRank)
                {
                    var promo = new MovesAvailable(MoveType.PAWN_PROMOTION, this.PieceType, p, this);
                    lock (SyncRoot.MovesLock)
                    {
                        SpecialMoves?.Add(promo);
                    }
                }

                lock (SyncRoot.MovesLock)
                {
                    AvailableMoves?.Add(new MovesAvailable(MoveType.NORMAL, this.PieceType, p, this));
                }
            }

            // If pawn hasn't moved: consider one- and two-square advances (two-square requires intermediate empty)
            if (!ChessPieceMoved)
            {
                var one = ChessPieceExtenstions.OneSquareForward(this.CurrentLocation.X + forward * 1, this.CurrentLocation.Y);
                var two = ChessPieceExtenstions.OneSquareForward(this.CurrentLocation.X + forward * 2, this.CurrentLocation.Y);

                if (CanAddForward(one))
                {
                    AddNormalMoveWithPromotion(one);

                    // only allow two-square if both squares are empty and pin allows the two-square target
                    if (CanAddForward(two) && IsSquareEmpty(two, squares))
                    {
                        if (PinAllows(two))
                            AvailableMoves?.Add(new MovesAvailable(MoveType.NORMAL, this.PieceType, two, this));
                    }
                }

                return;
            }

            // Pawn has moved: single square forward only
            var single = ChessPieceExtenstions.OneSquareForward(this.CurrentLocation.X + forward * 1, this.CurrentLocation.Y);
            AddNormalMoveWithPromotion(single);
        }

        /// <summary>
        /// Generates attack moves for the Pawn piece on the given chess board, considering available moves and opponent positions.
        /// </summary>
        /// <param name="chessBoard">The chess board on which to generate attack moves.</param>
        public void GenerateAttackMoves(Board chessBoard)
        {
            if (chessBoard?.ChessSquares_ == null) return;

            // Clean up previous state
            AttackMoves?.Clear();
            // Use helper to replace with an empty list under lock
            ReplaceAllAvailableMoves(null);
            SpecialMoves?.RemoveAll(s => s.MoveType != MoveType.EnPASSENT && s.MoveType != MoveType.PAWN_PROMOTION);

            // Find the opponent's king on the chess board
            var otherKing = ChessPieceExtenstions.FindChessPiece(
                PieceType.KING,
                ChessPieceExtenstions.GetOtherColor(this.PieceColor),
                chessBoard.ChessSquares_) as King;

            // Get the positions of the opponent's pieces based on the current piece's color
            var opponentChessPieceList = (this.PieceColor == ChessPieceColors.WHITE)
                ? chessBoard.BlackChessPieces
                : chessBoard.WhiteChessPieces;

            bool isPlayer = this.PieceColor == ChessGameService.playerColor;
            int forward = isPlayer ? -1 : +1;
            // Preserve your original promotion-row checks: original code used CurrentLocation.X == 1 for player and 6 for opponent
            int promotionFromRank = isPlayer ? 1 : 6;

            // local helper to handle one diagonal target
            void HandleTarget(ChessSquareLocation target)
            {
                bool occupiedByOpponent = opponentChessPieceList.Any(cp => cp.CurrentLocation == target);
                bool sameColor = this.IsSamePieceColor(target, opponentChessPieceList);

                if (occupiedByOpponent && !sameColor)
                {
                    if (target == otherKing?.CurrentLocation)
                    {
                        var m = new MovesAvailable(MoveType.CHECK, this.PieceType, target, this);
                        AttackMoves?.Add(m);
                        AvailableMoves?.Add(m);
                        AddAvailablePeek(new Peek(target, this.PieceType, this.PieceColor));
                    }
                    else if (this.CurrentLocation.X == promotionFromRank)
                    {
                        var m = new MovesAvailable(MoveType.PAWN_PROMOTION, this.PieceType, target, this);
                        SpecialMoves?.Add(m);
                        AvailableMoves?.Add(m);
                        AddAvailablePeek(new Peek(target, this.PieceType, this.PieceColor));
                    }
                    else
                    {
                        var m = new MovesAvailable(MoveType.CAPTURE, this.PieceType, target, this);
                        AttackMoves?.Add(m);
                        AvailableMoves?.Add(m);
                        AddAvailablePeek(new Peek(target, this.PieceType, this.PieceColor));
                    }
                }
                else
                {
                    AddAvailablePeek(new Peek(target, this.PieceType, this.PieceColor));
                }
            }

            // compute the two diagonal attack squares using the determined forward direction
            var attackRight = ChessPieceExtenstions.OneSquareDiagonal(this.CurrentLocation.X + forward, this.CurrentLocation.Y + 1);
            var attackLeft = ChessPieceExtenstions.OneSquareDiagonal(this.CurrentLocation.X + forward, this.CurrentLocation.Y - 1);

            if(attackRight.IsInBounds())
              HandleTarget(attackRight);
            if(attackLeft.IsInBounds())
              HandleTarget(attackLeft);

            // Defensive: ensure no null Peek entries remain in the backing list
            // (some code paths elsewhere may mistakenly insert nulls)
            ReplaceAllAvailableMoves(GetAllAvailableMovesSnapshot().Where(p => p != null));
        }

        /// <summary>
        /// Generates check moves for the Pawn piece on the given chess board, considering the attacker and path to the king.
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

                SpecialMoves?.RemoveAll(m => !m.Move.Equals(attackerTarget));
            }
            else
            {
                // No pathToKing & attacker is not short-range:
                // keep only attack/special moves that directly capture/check the attacker square
                AttackMoves?.RemoveAll(m => !m.Move.Equals(attackerTarget));

                SpecialMoves?.RemoveAll(m => !m.Move.Equals(attackerTarget));
            }
        }

        # endregion

        #region -- Helper Method -- 

        /// <summary>
        /// Marks the Pawn as having moved if its current location is different from its starting location.
        /// </summary>
        public void Moved()
        {
            // has pawn been moved
            if (StartLocation != CurrentLocation)
            {
                ChessPieceMoved = true;
            }
        }

        #endregion

        #region -- Thread-Safe Move Management -- 

        /// <summary>
        /// Adds a new Peek move to the list of all available moves for the Pawn piece, ensuring thread safety and avoiding null entries.
        /// </summary>
        /// <param name="p">The Peek move to add.</param>
        public void AddAvailablePeek(Peek? p)
        {
            if (p == null) return;
            lock (SyncRoot.MovesLock) // Ensure thread safety when adding to the list of all available moves
            {
                _allAvailableMoves ??= [];
                _allAvailableMoves.Add(p);
            }
        }

        /// <summary>
        /// Replaces the entire list of all available moves for the Pawn piece with a new collection, ensuring thread safety and avoiding null entries.
        /// </summary>
        /// <param name="items">The new collection of peek moves.</param>
        public void ReplaceAllAvailableMoves(IEnumerable<Peek>? items)
        {
            lock (SyncRoot.MovesLock) // Ensure thread safety when replacing the list of all available move
            {
                _allAvailableMoves = items?.Where(x => x != null).ToList() ?? [];
            }
        }

        /// <summary>
        /// Gets a snapshot of all available moves for the Pawn piece, ensuring thread safety and returning a copy of the list to avoid external modifications.
        /// </summary>
        /// <returns>A list of all available peek moves.</returns>
        public List<Peek> GetAllAvailableMovesSnapshot()
        {
            lock (SyncRoot.MovesLock) // Ensure thread safety when accessing the list of all available moves
            {
                return _allAvailableMoves == null ? [] : [.. _allAvailableMoves];
            }
        }

        #endregion

        #region -- Deep Clone Methods --
        /// <summary>
        /// Creates a deep clone of the Pawn piece, including its properties and available moves, ensuring that the clone is independent of the original instance.
        /// </summary>
        /// <returns>A deep clone of the Pawn piece.</returns>
        public ChessPiece DeepClone()
        {
            var clone = new Pawn(this.ChessGameService, this.PieceType, this.PieceColor,
                         this.SpecialListOfMoves,
                         this.CurrentLocation,
                         this.PieceImage)
            {
                StartLocation = this.StartLocation,
                ChessPieceMoved = this.ChessPieceMoved,
                AvailableMoves = this.AvailableMoves,
                SpecialMoves = this.SpecialMoves,
                AttackMoves = this.AttackMoves,
                AllAvailableMoves = this.AllAvailableMoves,
            };

            return clone;
        }

        /// <summary>
        /// Creates a deep clone of the Pawn piece used in ChessSquare operations, 
        /// including its properties and available moves, ensuring that the clone is independent of the original instance.
        /// </summary>
        /// <returns>A deep clone of the Pawn piece.</returns>
        public Pawn DeepClone_()
        {
            var clone = new Pawn(ChessGameService, PieceType, PieceColor, SpecialListOfMoves,
                new ChessSquareLocation(CurrentLocation.X, CurrentLocation.Y), PieceImage)
            {
                StartLocation = this.StartLocation,
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
