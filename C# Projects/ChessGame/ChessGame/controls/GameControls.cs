using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.SpecialMoves;
using ChessGame.ChessGameMoves.SpecialMoves.helpers;
using ChessGame.ChessPieces;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Services;
using ChessGame.Square;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace ChessGame.Controls
{
    /// <summary>
    /// Represents the main control class for managing the state and behavior of a chess game.
    /// </summary>
    public class GameControls : ObservableObject
    {
        #region -- Properties --
        public IIObjectService MainView { get; set; } = default!;
        public IIChessGameService? ChessGameService { get; set; } = default!;
        public AppSettingsService AppSettingService { get; set; } = default!;
        public Board ChessBoard { get; set; } = default!;
        public SpecialMoveHandler SpecialMoveHandler { get; set; } = default!;
        public PlayerControls PlayerControls { get; set; } = default!;
        public IsCheckControls IsCheckControls { get; set; } = default!;
        public MovementControls MovementControls { get; set; } = default!;
        public SettingControls SettingControls { get; set; } = default!;
        private Visibility _IsPawnSelectionVisible;
        public int MoveCountEnPassentEnabled { get; set; }
        public ObservableCollection<ChessPiece>? pawnPromotionChessPieces;
        public ObservableCollection<ChessPiece>? PawnPromotionChessPieces
        {
            get => pawnPromotionChessPieces;
            set
            {
                pawnPromotionChessPieces = value;
                OnPropertyChanged(nameof(PawnPromotionChessPieces));
            }
        }

        public Visibility IsPawnSelectionGridVisible
        {
            get => _IsPawnSelectionVisible;
            set
            {
                _IsPawnSelectionVisible = value;
                OnPropertyChanged(nameof(IsPawnSelectionGridVisible));
            }
        }

        #endregion

        #region -- Constructors --
        public GameControls() { }

        public GameControls(IIObjectService mainView, AppSettingsService appSettingService,
            Board chessBoard, IIChessGameService? chessGameService = null)
        {
            this.MainView = mainView;
            this.AppSettingService = appSettingService;
            this.ChessGameService = chessGameService;

            IsPawnSelectionGridVisible = Visibility.Collapsed;

            this.ChessBoard = chessBoard;

            PlayerControls = new PlayerControls(this);
            MovementControls = new MovementControls(this);
            SettingControls = new SettingControls(this);

            SpecialMoveHandler = new SpecialMoveHandler(this.ChessBoard, this.ChessGameService);
        }

        #endregion

        #region -- Methods --

        /// <summary>
        /// Finds the king chess piece of the specified color on the given chess board.
        /// </summary>
        /// <param name="color">The color of the king to find.</param>
        /// <param name="chessBoard">The chess board to search.</param>
        /// <returns>The king chess piece if found; otherwise, null.</returns>
        public static King? FindKingChessPiece(ChessPieceColors color, Board chessBoard)
        {
            if (chessBoard.ChessSquares_ != null)
            {
                foreach (var piece in chessBoard.ChessSquares_)
                {
                    // Check if the square contains a chess piece and if it's a king of the specified color
                    if (piece.ChessPiece_ != null && piece.ChessPiece_ is King king && king.PieceColor == color)
                    {
                        return king;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the path from the attacker chess piece to the king chess piece, excluding the king's square.
        /// </summary>
        /// <param name="king">The king chess piece.</param>
        /// <param name="attacker">The attacking chess piece.</param>
        /// <returns>A list of chess square locations representing the path from the attacker to the king, or null if the attacker is a knight.</returns>
        public static List<ChessSquareLocation>? GetPathToKing(King king, ChessPiece attacker)
        {
            // If the attacker is not a knight, calculate the path from the attacker to the king
            if (attacker.PieceType != PieceType.KNIGHT)
            {
                // Calculate the direction of movement from the attacker to the king
                var path = new List<ChessSquareLocation>();
                int dx = Math.Sign(king.CurrentLocation.X - attacker.CurrentLocation.X);
                int dy = Math.Sign(king.CurrentLocation.Y - attacker.CurrentLocation.Y);

                // Calculate the next square in the path based on the direction of movement
                int x = attacker.CurrentLocation.X + dx;
                int y = attacker.CurrentLocation.Y + dy;

                try
                {
                    while (x != king.CurrentLocation.X || y != king.CurrentLocation.Y)
                    {
                        // Add the current square to the path
                        path.Add(new ChessSquareLocation(x, y));
                        x += dx;
                        y += dy;

                        if (path.Count > 8)
                        {
                            break;
                        }
                    }

                    // Add the king's square to the path (optional, depending on your needs)
                    path.Add(new ChessSquareLocation(x, y));
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"ERORR! {e.Message}");
                }

                return path;
            }

            return null;
        }

        #endregion

        #region -- Castling Activity Monitoring --

        /// <summary>
        /// Monitors the castling activity on the chess board and updates the castling rights and special moves accordingly.
        /// </summary>
        /// <param name="chessBoard">The chess board to monitor.</param>
        public void MonitorCastlingActivity(Board chessBoard)
        {
            // take a snapshot of the castling pieces dictionary under lock to avoid
            // collection-modified exceptions if it is updated concurrently
            Dictionary<ChessPieceColors, Dictionary<CastlingSide, ChessPiece>> castlingSnapshot;
            lock (SyncRoot.MovesLock)
            {
                castlingSnapshot = SpecialMoveHandler.CastlingChessPieces != null
                    ? new Dictionary<ChessPieceColors, Dictionary<CastlingSide, ChessPiece>>(SpecialMoveHandler.CastlingChessPieces)
                    : new Dictionary<ChessPieceColors, Dictionary<CastlingSide, ChessPiece>>();
            }

            foreach (var sideToMove in castlingSnapshot)
            {
                // take a snapshot of the castling pieces for the current player under lock to avoid
                ChessPieceColors pieceColor = sideToMove.Key;
                Dictionary<CastlingSide, ChessPiece> castlingPieces = sideToMove.Value;

                King? king = null;
                Rook? rookQS = null;
                Rook? rookKS = null;

                // retrieve the king and rooks for the current player from the castling pieces dictionary
                if (castlingPieces.TryGetValue(CastlingSide.KINGCASTLE, out var kingPiece))
                    king = kingPiece as King;

                if (castlingPieces.TryGetValue(CastlingSide.QUEENSIDE, out var queenPiece))
                    rookQS = queenPiece as Rook;

                if (castlingPieces.TryGetValue(CastlingSide.KINGSIDE, out var kingSidePiece))
                    rookKS = kingSidePiece as Rook;

                if (king is King k
                    && king is ICastlingMove castlingChessPiece
                    && !string.IsNullOrWhiteSpace(king.PieceImage))
                {
                    // king has not moved, evaluate the current player's chess position on the board for a castling move
                    if (!k.ChessPieceMoved)
                    {
                        // evaluate the current player's chess position on the board for a castling move
                        (bool IsCastlingSafe, List<(ChessSquareLocation AttackedSquare, 
                            ChessSquareLocation RookChessPieceLoc)> SquaresAttacked) = EvaluateCasltingMove(king, rookQS, rookKS, chessBoard);

                        // check square to move to is safe using a definitely-assigned king
                        if (IsCastlingSafe && !king.IsChecked)
                        {
                            // generate the castling move for the king and rooks
                            castlingChessPiece.GenerateCasltingMove(chessBoard, king, rookQS, rookKS, SquaresAttacked);
                        }
                        else
                        {
                            rookKS?.SpecialMoves?.Clear();
                            rookQS?.SpecialMoves?.Clear();
                            king?.SpecialMoves?.Clear();
                        }
                    }
                    else
                    {
                        // king has moved, remove the current player's castling rights
                        if (k.PieceColor == ChessPieceColors.WHITE)
                        {
                            chessBoard.WhiteCastleRightsQueenSide = false;
                            chessBoard.WhiteCastleRightsKingSide = false;
                        }
                        else
                        {
                            chessBoard.BlackCastleRightsKingSide = false;
                            chessBoard.BlackCastleRightsQueenSide = false;
                        }

                        // clear the special moves for the king and rooks if they exist
                        if (king != null && king is IPieceSpecialMove kg)
                        {
                            kg?.SpecialMoves?.Clear();

                            if (rookQS is IPieceSpecialMove rqs && rookKS is IPieceSpecialMove rks)
                            {
                                rqs.SpecialMoves?.Clear();
                                rks.SpecialMoves?.Clear();
                            }
                        }
                    }
                }

                // remove the current player's castling rights if the rooks have moved
                if (rookQS != null && rookQS.ChessPieceMoved)
                {
                    if (rookQS.PieceColor is ChessPieceColors.WHITE)
                    {
                        rookQS?.SpecialMoves?.Clear();
                        chessBoard.WhiteCastleRightsQueenSide = false;
                    }
                    else
                    {
                        rookQS?.SpecialMoves?.Clear();
                        chessBoard.BlackCastleRightsQueenSide = false;
                    }

                    SpecialMoveHandler?.CastlingChessPieces?[pieceColor].Remove(CastlingSide.QUEENSIDE);
                }

                // remove the current player's castling rights if the rooks have moved
                if (rookKS != null && rookKS.ChessPieceMoved)
                {
                    if (rookKS.PieceColor is ChessPieceColors.WHITE)
                    {
                        rookKS.SpecialMoves?.Clear();
                        chessBoard.WhiteCastleRightsKingSide = false;
                    }
                    else
                    {
                        rookKS.SpecialMoves?.Clear();
                        chessBoard.BlackCastleRightsKingSide = false;
                    }

                    SpecialMoveHandler?.CastlingChessPieces?[pieceColor].Remove(CastlingSide.KINGSIDE);
                }

                // remove the current player's castling rights if the king has moved
                if (chessBoard.WhiteCastleRightsKingSide is false
                    && chessBoard.WhiteCastleRightsQueenSide is false
                    && chessBoard.BlackCastleRightsKingSide is false
                    && chessBoard.BlackCastleRightsQueenSide is false)
                {
                    
                    SpecialMoveHandler?.CastlingChessPieces?.Remove(pieceColor);
                }
            }
        }

        /// <summary>
        /// Evaluate the current player's chess board moves to determine if the castling square where the king is placed is safe
        /// while also evaluating on which side of the board the attacker is guarding
        /// </summary>
        /// <param name="king">The king piece to evaluate.</param>
        /// <param name="rookQS">The queen-side rook piece.</param>
        /// <param name="rookKS">The king-side rook piece.</param>
        /// <param name="chessBoard">The chess board to evaluate.</param>
        /// <returns> a tuple containing bool, list, and a tuple </returns>
        private (bool IsCastlingSafe, List<(ChessSquareLocation AttackedSquare, 
            ChessSquareLocation RookChessPieceLoc)> SquaresAttacked) EvaluateCasltingMove(King king, Rook? rookQS, Rook? rookKS, Board chessBoard)
        {

            List<ChessSquareLocation> rooksLocation = [];

            List<(ChessSquareLocation AttackedSquare, ChessSquareLocation RookChessPieceLoc)> SquaresAttacked = [];

            int row = king.CurrentLocation.X;

            // castling is only possible on the first and last row of the chess board
            if (row != 0 && row != 7)
                return (false, SquaresAttacked);

            // determine the affected squares based on the player's color
            var affectedSquares = (ChessGameService?.playerColor == ChessPieceColors.WHITE) ?
                new[]
                {
                    // king side is on the right side and queen side is on the left side
                    new ChessSquareLocation(row,2),
                    new ChessSquareLocation(row,3),
                    new ChessSquareLocation(row,5),
                    new ChessSquareLocation(row,6),

                } :

                [
                     // king side is on the left side and queen side is on the right side
                     new ChessSquareLocation(row,1),
                     new ChessSquareLocation(row,2),
                     new ChessSquareLocation(row,4),
                     new ChessSquareLocation(row,5),
                ];

            // snapshot the board squares under lock to prevent modifications while iterating
            List<ChessSquare> chessSquares;
            lock (SyncRoot.MovesLock)
            {
                chessSquares = chessBoard.ChessSquares_?.ToList() ?? [];
            }

            // moves an attackers can guard and the kings nearest rook to that chess square   
            var rookOptions = new[]
            {
                (Move: affectedSquares[0],
                 Rook: new ChessSquareLocation(row, 0)),

                (Move: affectedSquares[1],
                 Rook: new ChessSquareLocation(row, 0)),


                (Move: affectedSquares[2],
                 Rook: new ChessSquareLocation(row, 7)),

                (Move: affectedSquares[3],
                 Rook: new ChessSquareLocation(row, 7)),
            };

            foreach (var oppChessPiece in chessSquares)
            {
                if (oppChessPiece.ChessPiece_ is ChessPiece chessPiece
                    && chessPiece.PieceColor != king.PieceColor
                    && chessPiece is IChessMoves chessPieceMoves)
                {
                    // get the available moves for the opponent's chess piece
                    IEnumerable<IChessMove>? cp = chessPiece.PieceType is PieceType.PAWN ?
                        chessPieceMoves.AllAvailableMoves?.Cast<IChessMove>() : chessPieceMoves.AvailableMoves?.Cast<IChessMove>();

                    // take a snapshot of the moves collection under lock so Any/iteration is safe
                    List<IChessMove>? cpSnapshot;
                    lock (SyncRoot.MovesLock)
                    {
                        cpSnapshot = cp?.ToList();
                    }

                    // evaluate the moves that can guarded by the opponent
                    foreach (var evaluatePos in rookOptions)
                    {
                        // attacker is guarding kings caslting square 
                        bool attackCastlingSquare = cpSnapshot?.Any(
                            mv => mv is not null &&
                            (mv.Move == evaluatePos.Move) == true &&
                            ((rookQS != null && evaluatePos.Rook == rookQS.CurrentLocation) ||
                            (rookKS != null && evaluatePos.Rook == rookKS.CurrentLocation))) ?? false;

                        // if the opponent is guarding the castling square, add it to the list of attacked squares
                        if (attackCastlingSquare)
                            SquaresAttacked.Add((evaluatePos.Move, evaluatePos.Rook));
                        else
                            continue;

                        // add the rook's location to the list of rooks that are guarding the castling square
                        if (!rooksLocation.Any(r => r == evaluatePos.Rook))
                            rooksLocation.Add(evaluatePos.Rook);
                    }

                }
            }

            // if the number of rooks guarding the castling square is less than or equal to 1, then the castling move is safe
            return (rooksLocation.Count <= 1, SquaresAttacked);
        }

        # endregion

        #region -- Pawn Promotion Activity Monitoring --

        /// <summary>
        /// Monitors the pawn promotion activity on the chess board and updates the pawn promotion selection grid visibility 
        /// and available chess pieces for promotion.
        /// </summary>
        /// <param name="chessBoard">The chess board to monitor.</param>
        public void MonitorPawnPromotedMoveActivity(Board chessBoard)
        {
            if (chessBoard.ChessSquares_ != null && ChessGameService?.selectedGameMode is not null
                && !ChessGameService.selectedGameMode.Equals("AI-AI"))
            {
                // take a snapshot of the pawn pieces dictionary under lock to avoid
                Dictionary<ChessPieceColors, Dictionary<int, Pawn?>> pawnSnapshot;
                lock (SyncRoot.MovesLock)
                {
                    pawnSnapshot = SpecialMoveHandler.PawnChessPieces != null
                        ? new Dictionary<ChessPieceColors, Dictionary<int, Pawn?>>(SpecialMoveHandler.PawnChessPieces)
                        : [];
                }

                foreach (var sideToMove in pawnSnapshot.ToDictionary() ?? [])
                {
                    // take a snapshot of the pawn pieces for the current player under lock to avoid
                    ChessPieceColors pieceColor = sideToMove.Key;
                    Dictionary<int, Pawn?> PawnPieces = sideToMove.Value;

                    foreach (var pawn in PawnPieces.ToDictionary() ?? [])
                    {
                        int index = pawn.Key;
                        Pawn? p = pawn.Value;

                        if (p is null)
                            continue;

                        // check if the pawn has reached the promotion rank and if it belongs to the current player
                        if (PawnPromotion.IsPawnPromoted(p, p.CurrentLocation) && p.PieceColor == ChessGameService?.playerColor)
                        {
                            // set the pawn promotion selection flag to true and clear the available chess pieces for promotion
                            if (ChessGameService != null) ChessGameService.IsPawnPromotionSelection = true;
                            PawnPromotionChessPieces = [];
                            
                            IEnumerable<ChessPiece> pawnPromotionChessPieces_;

                            // generate the available chess pieces for promotion based on the pawn's color and the selected chess set
                            if (p.PieceColor == ChessPieceColors.WHITE)
                            {
                                pawnPromotionChessPieces_ = ChessPieceExtenstions.PawnPromotionChessPieces(
                                    AppSettingService.Settings.ChessSetSelected().whitePieces, p, p.CurrentLocation);
                            }
                            else
                            {
                                pawnPromotionChessPieces_ = ChessPieceExtenstions.PawnPromotionChessPieces(AppSettingService.Settings.ChessSetSelected().blackPieces, p,
                                    p.CurrentLocation);
                            }

                            // set the available chess pieces for promotion to the generated list
                            PawnPromotionChessPieces = new ObservableCollection<ChessPiece>(pawnPromotionChessPieces_);
                            IsPawnSelectionGridVisible = Visibility.Visible;

                            // remove the pawn from the pawn pieces dictionary to prevent it from being promoted again
                            if (pawnPromotionChessPieces_.Any() &&
                                SpecialMoveHandler?.PawnChessPieces?.ContainsKey(pieceColor) == true
                                && SpecialMoveHandler.PawnChessPieces[pieceColor].ContainsKey(index))
                            {
                                SpecialMoveHandler.PawnChessPieces[pieceColor].Remove(index);
                            }
                        }
                    }
                }
            }
        }

        # endregion

        #region -- En Passant Activity Monitoring --

        /// <summary>
        /// Monitors the en passant activity on the chess board and updates the en passant target square and special moves accordingly.
        /// </summary>
        /// <param name="MoveCount">The current move count in the game.</param>
        /// <param name="chessBoard">The chess board to monitor.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void MonitorEnPassantActivity(int MoveCount, Board chessBoard)
        {
            if (chessBoard.ChessSquares_ != null && !SpecialMoveHandler.AllPawnsMoved)
            {
                // take a snapshot of the pawn pieces dictionary under lock to avoid
                Dictionary<ChessPieceColors, Dictionary<int, Pawn?>> pawnSnapshot;
                lock (SyncRoot.MovesLock)
                {
                    pawnSnapshot = SpecialMoveHandler.PawnChessPieces != null
                        ? new Dictionary<ChessPieceColors, Dictionary<int, Pawn?>>(SpecialMoveHandler.PawnChessPieces)
                        : [];
                }

                bool enPassantRemoved = false;
                foreach (var sideToMove in pawnSnapshot.ToDictionary() ?? [])
                {
                    ChessPieceColors pieceColor = sideToMove.Key;
                    Dictionary<int, Pawn?> PawnPieces = sideToMove.Value;

                    foreach (var pawn in PawnPieces.ToDictionary() ?? [])
                    {
                        int index = pawn.Key;
                        Pawn? p = pawn.Value;

                        if (p is null)
                            continue;

                        // check if the pawn has moved two squares forward and if it belongs to the current player
                        if (SpecialMoveHandler is null)
                            throw new InvalidOperationException($"{nameof(SpecialMoveHandler)} is null.");

                        // check if the pawn has moved two squares forward and if it belongs to the current player
                        if (SpecialMoveHandler.EnPassant.IsFirstMoveEnPassant(p, p.StartLocation, p.CurrentLocation, p.ChessPieceMoved))
                        {
                            // add the en passant target square to the opponent's special moves if the pawn has moved two squares forward
                            (bool IsAddEnPassantAttack, ChessPieceColors? IsAddToPlayer, string? ChessBoardFileRank, ChessPiece? Pawn)
                                = SpecialMoveHandler.EnPassant.AddEnPassentSquareToOpponent(p, chessBoard.ChessSquares_);

                            // if the en passant target square is valid, set the en passant target square on the chess board
                            // and update the en passant move count
                            if (IsAddEnPassantAttack && ChessBoardFileRank is not null)
                            {
                                chessBoard.EnPassantTarget = new EnPassantHelper(ChessBoardFileRank, MoveCount, null, p.PieceColor, IsAddToPlayer, Pawn);
                                SpecialMoveHandler.EnPassant.EnPassantMoveSetCount = MoveCount;
                            }
                        }

                        // remove the en passant target square from the opponent's special moves if the pawn has moved
                        // and the en passant target square is no longer valid
                        if (p.CanCaptureEnpassant && 
                            p.SpecialMoves != null && 
                            chessBoard.EnPassantTarget.ResetColor == chessBoard.EnPassantTarget.SetForPlayer)
                        {
                            p.SpecialMoves.Clear();
                            p.CanCaptureEnpassant = false;

                            enPassantRemoved = true;
                        }
                    }

                    // remove the en passant target square from the chess board if it is no longer valid
                    if (enPassantRemoved)
                    {
                        SpecialMoveHandler.EnPassant.EnPassantMoveEnabled = false;
                        chessBoard.EnPassantTarget = new EnPassantHelper("-", MoveCount, null, null, null, null);
                    }

                    // check if all pawns have moved and update the AllPawnsMoved flag accordingly
                    if (MoveCount > (SpecialMoveHandler.EnPassant.EnPassantMoveSetCount) &&
                        SpecialMoveHandler.PawnChessPieces != null)
                    {

                        SpecialMoveHandler.AllPawnsMoved = SpecialMoveHandler.PawnChessPieces
                            .SelectMany(pieceColor => pieceColor.Value.Values)
                            .All(pawn => pawn is not null && pawn.ChessPieceMoved);
                    }
                }
            }
        }

        #endregion

        #region -- Reset Method --

        /// <summary>
        /// Resets the game controls, including resetting the tracking of special moves and chess pieces.
        /// </summary>
        /// <returns>A task that represents the asynchronous reset operation.</returns>
        public async Task Reset()
        {
            // reset tracking chess pieces
            SpecialMoveHandler.Reset();
        }

        #endregion

    }
}