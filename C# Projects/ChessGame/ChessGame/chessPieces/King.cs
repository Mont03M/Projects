using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.PeekMoves;
using ChessGame.ChessGameMoves.SpecialMoves;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Square;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Collections.ObjectModel;

namespace ChessGame.ChessPieces
{
    /// <summary>
    /// Represents a King chess piece, inheriting from ChessPiece and implementing IChessMoves, 
    /// ICastlingMove, IPieceSpecialMove, and IMoved interfaces.
    /// </summary>
    public class King : ChessPiece, IChessMoves, ICastlingMove, IPieceSpecialMove,  IMoved
    {
        #region -- Properties ---
        private bool IsCheckedPiece { get; set; } = false;
        public ChessSquareLocation StartLocation { get; set; }
        private bool HasMoved_ { get; set; } = false;
        private List<MovesAvailable>? availableMoves_;
        public List<MovesAvailable>? AttackMoves { get; set; }
        public List<MovesAvailable>? SpecialMoves { get; set; }
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

        // special move
        public Special_Moves Caslting { get; set; }

        public List<MovesAvailable>? AvailableMoves 
        { 
            get=>availableMoves_;
            set
            {
                availableMoves_ = value;
                OnPropertyChanged(nameof(AvailableMoves));
            }
        }

        public bool IsChecked 
        {
            get => IsCheckedPiece;
            set
            {
                IsCheckedPiece = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

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

        #region -- Constructor ---
        public King(IIChessGameService chessGameService, PieceType pieceType,
           ChessPieceColors pieceColor, Special_Moves caslting, ChessSquareLocation currentLocation, string pieceImage)
           : base(chessGameService, pieceType, pieceColor, currentLocation, pieceImage)
        {
            AttackMoves = [];
            AvailableMoves = [];
            SpecialMoves = [];
            AllAvailableMoves = [];

            this.StartLocation = currentLocation;
            this.ChessPieceMoved = false;
            this.Caslting = caslting;
        }
        #endregion

        #region -- Main Methods --

        /// <summary>
        /// This method does not implement any functionality.
        /// </summary>
        /// <param name="chessBoard"> the chess board on which to generate moves </param>
        /// <exception cref="NotImplementedException"> Not implemented. </exception>
        public void GenerateValidMoves(Board chessBoard)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates the available moves for the King piece based on the current state of the chessboard.
        /// </summary>
        /// <param name="chessBoard">The chessboard</param>
        public void GenerateAvailableMoves(Board chessBoard)
        {
            if(chessBoard.ChessSquares_ == null) return;

            if(AvailableMoves?.Count > 0)
            {
                lock (SyncRoot.MovesLock) // Ensure thread safety when clearing the list
                {
                    AvailableMoves.Clear();
                }
            }

            // Clear backing list in a thread-safe manner (uses its own lock)
            ReplaceAllAvailableMoves(null);

            Moved();

            // Generate all possible moves for the King piece
            var kingMoves = this.KingMoves([]);

            // Get the list of chess pieces for the current player based on the King's color
            var playerChessPieces = ((this.PieceColor == ChessPieceColors.WHITE)
              ? chessBoard.WhiteChessPieces
              : chessBoard.BlackChessPieces);
            foreach (var move in kingMoves)
            {
                // Check if the move is within bounds and not occupied by a piece of the same color
                if (move.IsInBounds() && 
                    !this.IsSamePieceColor(move, playerChessPieces))
                {
                    lock (SyncRoot.MovesLock) // Ensure thread safety when adding to the list
                    {
                        AvailableMoves?.Add(new MovesAvailable(MoveType.NORMAL, this.PieceType, move, this));
                    }
                }
            }

            // Peek ahead to check for moves that would place the King in check
            Peek(chessBoard.ChessSquares_);
        }

        /// <summary>
        /// Peeks ahead to determine if any of the King's available moves would place it in check or intersect with the other King.
        /// </summary>
        /// <param name="chessBoard"> a chess board of chessSquares.</param>
        public void Peek(ObservableCollection<ChessSquare> chessBoard)
        {
            // Get all possible moves for all pieces on the board
            var allMoves = ChessPieceExtenstions.AllMoves(chessBoard);

            // Find the other King on the board (opposite color)
            King? otherKing = ChessPieceExtenstions.FindChessPiece(PieceType.KING, ChessPieceExtenstions.GetOtherColor(this.PieceColor), chessBoard) as King;

            if (AvailableMoves?.Count > 0)
            {
                // collect moves to remove to avoid modifying the collection while enumerating
                var toRemove = new List<MovesAvailable>();
                var otherToRemove = new List<MovesAvailable>();

                // iterate a snapshot to avoid concurrent modification
                List<MovesAvailable> availableSnapshot;
                lock (SyncRoot.MovesLock)
                {
                    availableSnapshot = AvailableMoves?.ToList() ?? [];
                }

                // check king moves do not place itself in check mate
                foreach (var placesKingInCheckMove in availableSnapshot)
                {
                    // check if move can place the king in check mate if moved to that location
                    // peek ahead and check move can not be attacked by another chess piece
                    if (PlacesKingInCheck(placesKingInCheckMove.Move, this.PieceColor, chessBoard)
                        || PeekAhead(placesKingInCheckMove.Move, allMoves))
                    {
                        toRemove.Add(placesKingInCheckMove);
                    }

                    // check locations that kings intersect and remove moves from both kings
                    if (KingsIntersect(placesKingInCheckMove.Move, otherKing?.CurrentLocation))
                    {
                        toRemove.Add(placesKingInCheckMove);
                        if (otherKing?.AvailableMoves?.Count > 0)
                        {
                            otherToRemove.Add(placesKingInCheckMove);
                        }
                    }
                }

                // remove collected moves from this king
                foreach (var rem in toRemove)
                {
                    AvailableMoves?.Remove(rem);
                }

                // remove collected moves from the other king, if present
                if (otherKing != null && otherToRemove.Count > 0 && otherKing.AvailableMoves != null)
                {
                    foreach (var rem in otherToRemove)
                    {
                        otherKing.AvailableMoves?.Remove(rem);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the attack moves for the King piece based on its available moves and the opponent's pieces on the chessboard.
        /// </summary>
        /// <param name="chessBoard">The chess board containing the chess pieces.</param>
        public void GenerateAttackMoves(Board chessBoard)
        {
            if (chessBoard.ChessSquares_ == null) return;

            if (AttackMoves?.Count > 0)
                AttackMoves.Clear();

            // Get the list of opponent chess pieces based on the King's color
            var opponentChessPieceList = (this.PieceColor is ChessPieceColors.WHITE) ? chessBoard.BlackChessPieces : chessBoard.WhiteChessPieces;

            // Check if there are available moves to evaluate for attack opportunities
            if (AvailableMoves?.Count > 0)
            {
                if (chessBoard != null)
                {
                    foreach (var move in AvailableMoves.ToList())
                    {
                        // check if avaiable moves contains attackable moves 
                        if (opponentChessPieceList.Any(cp => cp.CurrentLocation == move.Move))
                        {
                            // If the move is an attackable move, set its MoveType to CAPTURE and add it to the AttackMoves list
                            move.MoveType = MoveType.CAPTURE;
                            AttackMoves?.Add(move);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates the valid moves for the King piece when it is in check, removing any moves that would place it in check or intersect with the other King.
        /// </summary>
        /// <param name="attacker">The attacking piece.</param>
        /// <param name="pathToKing">The path from the attacker to the King.</param>
        /// <param name="chessBoard">The chess board containing the chess pieces.</param>
        public void GenerateIsCheckMoves(ChessPiece attacker, List<ChessSquareLocation>? pathToKing, Board chessBoard = null!)
        {
            if (attacker is null) return;

            if (SpecialMoves?.Count > 0)
                SpecialMoves?.Clear();

            // Remove any available moves that would place the King in check or intersect with the other King
            if (attacker is IChessMoves chessPieceAttacker && 
                chessPieceAttacker is not null &&
                chessPieceAttacker.AvailableMoves?.Count > 0)
            {
                foreach (var move in AvailableMoves?.ToList() ?? [])
                {
                    // check if the move is in the attacker's available moves, and if so, remove it from the King's available moves
                    if (chessPieceAttacker.AvailableMoves.Contains(move))
                    {
                        AvailableMoves?.Remove(move);
                    }
                }
            }
        }

        #endregion

        #region -- King Check Detection Methods -- 

        /// <summary>
        /// Checks if a given move would place the King in check by any of the opponent's pieces on the chessboard.
        /// </summary>
        /// <param name="move">The move to check.</param>
        /// <param name="pieceColor">The color of the piece making the move.</param>
        /// <param name="chessBoard">The chess board containing the chess pieces.</param>
        /// <returns></returns>
        public static bool PlacesKingInCheck(ChessSquareLocation move, ChessPieceColors pieceColor, 
            ObservableCollection<ChessSquare> chessBoard)
        {
            if (chessBoard == null || chessBoard.Count == 0)
                return false;

            // Iterate all pieces on the board and check any attack-able square that matches 'move'.
            // Use attackMoves first (captures/attacks) and fall back to availableMoves for
            // pieces that treat their available moves as attack squares. This ensures pawns
            // (which usually record attack squares separately) are considered.
            foreach (var square in chessBoard)
            {
                if (square.ChessPiece_ == null)
                    continue;

                if (square.ChessPiece_ is not ChessPiece chessPiece)
                    continue;

                if (chessPiece.PieceColor == pieceColor)
                    continue; // only consider opponent pieces

                // Prefer attackMoves where present (captures/attacks). If present, check them.
                if (chessPiece is IChessMoves cp)
                {
                    if (cp.AttackMoves != null && cp.AttackMoves.Count > 0)
                    {
                        foreach (var a in cp.AttackMoves)
                        {
                            if (a.Move == move)
                                return true;
                        }
                    }

                    // Fallback: some pieces expose attack squares via availableMoves
                    if (cp.AvailableMoves != null && cp.AvailableMoves.Count > 0)
                    {
                        foreach (var a in cp.AvailableMoves)
                        {
                            if (chessPiece.PieceType is PieceType.PAWN && a.MoveType is MoveType.NORMAL)
                                continue;

                            if (a.Move == move)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a given move would place the King in check by any of the opponent's pieces on the chessboard.
        /// </summary>
        /// <param name="kingMove">The move to check.</param>
        /// <param name="legalBoardMoves">The list of legal board moves.</param>
        /// <returns>True if the move would place the King in check, false otherwise.</returns>
        public bool PeekAhead(ChessSquareLocation kingMove,  List<Peek> legalBoardMoves)
        {
            if (legalBoardMoves.Count > 0)
            {
                foreach (var check in legalBoardMoves)
                {
                    // checks moves that are not part of this object
                    if (check.Move == kingMove && 
                        this.PieceType != check.PieceType && 
                        this.PieceColor != check.Color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the two Kings intersect on the chessboard, meaning they are adjacent to each other.
        /// </summary>
        /// <param name="king">The location of the first king.</param>
        /// <param name="otherKing">The location of the second king.</param>
        /// <returns>True if the kings intersect, false otherwise.</returns>
        public static bool KingsIntersect(ChessSquareLocation king, ChessSquareLocation? otherKing)
        {
            if (otherKing == null)
                return false;

            // Calculate the absolute differences in X and Y coordinates between the two kings
            int dx = Math.Abs(king.X - otherKing.Value.X);
            int dy = Math.Abs(king.Y - otherKing.Value.Y);

            // Return true if the kings are adjacent (dx <= 1 and dy <= 1), indicating they intersect
            return dx <= 1 && dy <= 1;
        }

        #endregion

        #region -- Castling Moves Generation Method --

        /// <summary>
        /// Generates the caslting moves, added to the kings special moves
        /// Method determines which side of the chess board a castling move is allowed
        /// elimates caslting moves that places the king in check
        /// </summary>
        /// <param name="chessBoard"> Chess Squares </param>
        /// <param name="king"> Chess Squares </param>
        /// <param name="rookQS"> a rook chess piece on the queen side</param>
        /// <param name="rookKS"> a rook chess piece on the king side</param>
        /// <param name="SquaresAttacked"> Guard chess squares and rook locations</param>
        public void GenerateCasltingMove(Board chessBoard, King? king, Rook? rookQS, Rook? rookKS, 
            List<(ChessSquareLocation AttackedSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked)
        {
            if (chessBoard != null && king is King k)
            {
                // chess squares contain chess pieces
                bool IsCastleMoveRook1 = false;
                bool IsCastleMoveRook2 = false;

                // determine if rooks 1 can perform caslting moves
                if (rookQS is not null && king is not null && rookQS.CastleSide is CastlingSide.QUEENSIDE && SquaresAttacked?.Count >= 0)
                {
                    // helper
                    IsCastleMoveRook1 = Castling.IsCaslting(rookQS, k, SquaresAttacked, chessBoard);

                    // remove caslting moves if rook 1 can not perform caslting moves
                    if (!IsCastleMoveRook1 && rookQS is IPieceSpecialMove rQS && rookQS.SpecialMoves?.Count > 0)
                    {
                        // remove caslting moves from kings special moves if rook 1 can not perform caslting moves
                        SpecialMoves = SpecialMoves?.Where(km => km.Move != rookQS.StartLocation).ToList();

                        rQS.SpecialMoves?.Clear();
                    }
                }

                // determine if rooks 2 can perform caslting moves
                if (rookKS is not null && king is not null && rookKS.CastleSide is CastlingSide.KINGSIDE && SquaresAttacked?.Count >= 0)
                {
                    // helper
                    IsCastleMoveRook2 = Castling.IsCaslting(rookKS, k, SquaresAttacked, chessBoard);

                    if (!IsCastleMoveRook2 && rookKS is IPieceSpecialMove rKS && rookKS.SpecialMoves?.Count > 0)
                    {
                        // remove caslting moves from kings special moves if rook 2 can not perform caslting moves
                        SpecialMoves = SpecialMoves?.Where(km => km.Move != rookKS.StartLocation).ToList();

                        rKS.SpecialMoves?.Clear();
                    }
                }

                // both sides of current players board position can perform caslting moves
                if (IsCastleMoveRook1 &&
                    IsCastleMoveRook2 &&
                    rookQS is not null &&
                    rookKS is not null &&
                    king is not null &&
                    Castling.CasltingLocations(rookQS, king) is (ChessSquareLocation, ChessSquareLocation) moveRookQS &&
                    Castling.CasltingLocations(rookKS, king) is (ChessSquareLocation, ChessSquareLocation) moveRookKS)
                {
                    ArgumentNullException.ThrowIfNull(chessBoard.ChessSquares_);

                    // check if king has already added caslting moves to special moves
                    if (SpecialMoves?.Count >= 2)
                        return;

                    // check if rooks have already added caslting moves to special moves
                    var kingCastleSquareQS = moveRookQS.king.GetSquare(chessBoard.ChessSquares_);
                    var kingCastleSquareKS = moveRookKS.king.GetSquare(chessBoard.ChessSquares_);
                    var rookCastleSquareQS = moveRookQS.rook.GetSquare(chessBoard.ChessSquares_);
                    var rookCastleSquareKS = moveRookKS.rook.GetSquare(chessBoard.ChessSquares_);

                    // check if all squares are valid for caslting moves
                    if (kingCastleSquareQS != null && kingCastleSquareKS != null &&
                        rookCastleSquareQS != null && rookCastleSquareKS != null)
                    {
                        // king QS castle squares
                        SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, king.PieceType,
                            rookQS.StartLocation, king, rookQS,
                            kingCastleSquareQS, rookCastleSquareQS));

                        // king KS castle square
                        SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, king.PieceType,
                            rookKS.StartLocation, king, rookKS,
                            kingCastleSquareKS, rookCastleSquareKS));

                        // Rook QS castle square
                        (rookQS as IPieceSpecialMove).SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, rookQS.PieceType,
                          king.StartLocation, rookQS, king,
                          kingCastleSquareQS, rookCastleSquareQS));

                        // Rook KS castle square
                        (rookKS as IPieceSpecialMove).SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, rookKS.PieceType,
                          king.StartLocation, rookKS, king,
                          kingCastleSquareKS, rookCastleSquareKS));
                    }
                }
                // one side of the players board position can perform caslting moves
                else if (IsCastleMoveRook1 && king is not null && rookQS is not null &&
                    Castling.CasltingLocations(rookQS, king) is (ChessSquareLocation, ChessSquareLocation) movesRookQS)
                {
                    ArgumentNullException.ThrowIfNull(chessBoard.ChessSquares_);

                    // check if rook has already added caslting moves to special moves
                    if (rookQS is IPieceSpecialMove rQS && rQS.SpecialMoves?.Count >= 1)
                        return;

                    // check if king has already added caslting moves to special moves
                    var kingCastleSquareQS = movesRookQS.king.GetSquare(chessBoard.ChessSquares_);
                    var rookCastleSquareQS = movesRookQS.rook.GetSquare(chessBoard.ChessSquares_);

                    // check if all squares are valid for caslting moves
                    if (kingCastleSquareQS != null && rookCastleSquareQS != null)
                    {
                        // king castle square
                        SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, king.PieceType,
                            rookQS.StartLocation, king, rookQS
                            , kingCastleSquareQS, rookCastleSquareQS));

                        // rook castle square
                        (rookQS as IPieceSpecialMove).SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, rookQS.PieceType,
                            king.StartLocation, rookQS, king
                            , kingCastleSquareQS, rookCastleSquareQS));
                    }
                }
                // one side of the players board position can perform caslting moves
                else if (IsCastleMoveRook2 && king is not null && rookKS is not null &&
                    Castling.CasltingLocations(rookKS,king) is (ChessSquareLocation, ChessSquareLocation) movesRookKS)
                {
                    ArgumentNullException.ThrowIfNull(chessBoard.ChessSquares_);

                    // check if rook has already added caslting moves to special moves
                    if (rookKS is IPieceSpecialMove rKS && rKS.SpecialMoves?.Count >= 1)
                        return;

                    // check if king has already added caslting moves to special moves
                    var kingCastleSquareKS = movesRookKS.king.GetSquare(chessBoard.ChessSquares_);
                    var rookCastleSquareKS = movesRookKS.rook.GetSquare(chessBoard.ChessSquares_);

                    // check if all squares are valid for caslting moves
                    if (kingCastleSquareKS != null && rookCastleSquareKS != null)
                    {
                        SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, king.PieceType,
                            rookKS.StartLocation, king, rookKS,
                            kingCastleSquareKS, rookCastleSquareKS));

                        (rookKS as IPieceSpecialMove).SpecialMoves?.Add(new MovesAvailable(MoveType.CASTLING, rookKS.PieceType,
                            king.StartLocation, rookKS, king,
                            kingCastleSquareKS, rookCastleSquareKS));
                    }
                }
                else
                {
                    if(SpecialMoves?.Count > 0)
                    {
                        SpecialMoves.Clear();
                    }
                }
            }
        }

        #endregion

        #region -- Helpers ---

        /// <summary>
        /// Gets the squares of the rooks for castling based on the King's current location.
        /// </summary>
        /// <param name="chessBoard"> a chess board of containing chess squares.</param>
        /// <returns></returns>
        public (ChessSquare? rookQS, ChessSquare? rookKS) GetRookSquares(ObservableCollection<ChessSquare> chessBoard)
        {
            // Check if the King's current location is on the first or last column (X = 0 or X = 7)
            if (this.CurrentLocation.X == 0 || this.CurrentLocation.X == 7)
            {
                return (new ChessSquareLocation(CurrentLocation.X, 0).GetSquare(chessBoard), 
                    new ChessSquareLocation(CurrentLocation.X, 7).GetSquare(chessBoard));
              
            }

            return (null,null);
        }

        /// <summary>
        /// Marks the King as having moved if its current location is different from its starting location.
        /// </summary>
        public void Moved()
        {
            // has king moved
            if (StartLocation != this.CurrentLocation)
            {
                ChessPieceMoved = true;
            }
        }

        #endregion

        #region -- Thread-Safe Move Management -- 

        /// <summary>
        /// Adds a new Peek move to the list of all available moves for the King piece, ensuring thread safety and avoiding null entries.
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
        /// Replaces the entire list of all available moves for the King piece with a new collection, ensuring thread safety and avoiding null entries.
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
        /// Gets a snapshot of all available moves for the King piece, ensuring thread safety and returning a copy of the list to avoid external modifications.
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
        /// Creates a deep clone of the King object, including its properties and available moves, ensuring that the cloned object is independent of the original.
        /// </summary>
        /// <returns>A deep clone of the King object.</returns>
        public King DeepClone_()
        {
            // Create a new King object with the same properties as the current object
            var clone = new King(ChessGameService, PieceType, PieceColor, Caslting,
                new ChessSquareLocation(CurrentLocation.X, CurrentLocation.Y), PieceImage)
            {
                StartLocation = this.StartLocation,
                Caslting = this.Caslting,
                ChessPieceMoved = this.ChessPieceMoved,
                AvailableMoves = this.AvailableMoves != null ? [.. this.AvailableMoves] : null,
                SpecialMoves = this.SpecialMoves != null ? [.. this.SpecialMoves] : null,
                AttackMoves = this.AttackMoves != null ? [.. this.AttackMoves] : null,
                AllAvailableMoves = this.AllAvailableMoves != null ? [.. this.AllAvailableMoves] : null
            };

            return clone;
        }

        #endregion

    }
}
