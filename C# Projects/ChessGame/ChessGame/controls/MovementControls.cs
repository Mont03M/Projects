using ChessGame.ChessBoard;
using ChessGame.Square;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessGameMoves.SpecialMoves;
using ChessGame.ChessPieces;
using ChessGame.Controls.IsCheckHelper;
using ChessGame.Structs;
using ChessGame.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using ChessGame.Interfaces.Game;
using ChessGame.Game.chessSoundPlayer;

namespace ChessGame.Controls
{
    /// <summary>
    /// The MovementControls class manages the movement of chess pieces on the chessboard. 
    /// It handles generating available moves, performing piece moves, and managing special moves like castling. 
    /// It also provides events for animating piece movements and updating the board state. The class interacts with the GameControls to access the current game state and settings.
    /// </summary>
    public class MovementControls: ObservableObject
    {
        #region -- Properties --
        private GameControls GameControls { get; set; } = default!;

        public event Action<Action, Action, (ChessSquareLocation, ChessSquareLocation),
            (ChessSquareLocation, ChessSquareLocation), (string, string)>? OnPiecesAnimateCasltingMove;

        public event Action<Action, ChessSquareLocation, ChessSquareLocation, string>? OnPieceMoveAnimated;

        private Pawn? clonePiece;
        public bool IsPawnSelectedChessPiece { get; set; }
        public Pawn? ClonePiece
        {
            get => clonePiece;
            set
            {
                clonePiece = value;
                OnPropertyChanged(nameof(ClonePiece));
            }
        }

        #endregion

        #region -- Constructors --
        public MovementControls() { }

        public MovementControls(GameControls gameControls)
        {
            GameControls = gameControls;
        }

        #endregion

        #region -- Methods --

        /// <summary>
        /// Checks if a given chess board location is present in the list of available moves for a chess piece.
        /// </summary>
        /// <param name="chessPieceMoves">The list of available moves for a chess piece.</param>
        /// <param name="chessBoardLocation">The chess board location to check.</param>
        /// <returns>True if the location is present in the list of available moves; otherwise, false.</returns>
        public static bool ContainsMove(List<MovesAvailable> chessPieceMoves, ChessSquareLocation chessBoardLocation)
        {
            if (chessPieceMoves != null)
            {
                foreach (var move in chessPieceMoves)
                {
                    if (move.Move == chessBoardLocation)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a given chess board location is present in the list of available moves for a chess piece and returns the corresponding move if found.
        /// </summary>
        /// <param name="chessPieceMoves">The list of available moves for a chess piece.</param>
        /// <param name="chessBoardLocation">The chess board location to check.</param>
        /// <param name="foundMove">The corresponding move if found; otherwise, null.</param>
        /// <returns>True if the location is present in the list of available moves; otherwise, false.</returns>
        public static bool ContainsMove(List<MovesAvailable> chessPieceMoves, ChessSquareLocation chessBoardLocation, out MovesAvailable? foundMove)
        {
            foundMove = null;

            if (chessPieceMoves != null)
            {
                foreach (var move in chessPieceMoves)
                {
                    if (move.Move.Equals(chessBoardLocation))
                    {
                        foundMove = move;

                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region -- Generate Board Moves Method --
        /// <summary>
        /// Generates the available moves for all chess pieces on the given chess board. 
        /// This method invokes the BoardMoves method of the Board class on the UI thread to ensure thread safety when updating the board state.
        /// </summary>
        /// <param name="chessBoard">The chess board for which to generate available moves.</param>
        public static void GenerateBoardMoves(Board chessBoard)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                chessBoard.BoardMoves();
            });
        }

        #endregion

        #region -- Check State Movement Method --

        /// <summary>
        /// Generates the available moves for all chess pieces on the given chess board, considering the check state of the king.
        /// </summary>
        /// <param name="king">The king piece to consider for check state.</param>
        /// <param name="targetChessPieces">The list of target chess pieces to consider.</param>
        public void GenerateIsCheckMoves(King king, List<ChessPiece> targetChessPieces = null!)
        {
            // Do nothing if no attacker provided
            if (targetChessPieces is null) return;

            // Create a deep clone of the board for safe simulation so we don't mutate
            // the live board or its collections while computing check-filtered moves.
            var clonedBoard = GameControls.ChessBoard.DeepClone();

            foreach (var targetChessPiece in targetChessPieces)
            {
                // Find the corresponding target piece in the cloned board by location
                ChessPiece? clonedTarget = null;
                var targetLocation = targetChessPiece.CurrentLocation;

                if (clonedBoard.ChessSquares_ != null)
                {
                    var targetSquare = clonedBoard.ChessSquares_?.FirstOrDefault(s => s.BoardLocation == targetLocation);
                    clonedTarget = targetSquare?.ChessPiece_;
                }

                if (clonedTarget is null) return;

                // Get the path from the attacker to the king to determine which squares are under threat
                var pathToKing = GameControls.GetPathToKing(king, targetChessPiece);

                // Iterate a snapshot of cloned board squares to avoid collection-modified issues
                var squares = (clonedBoard.ChessSquares_ ?? []).ToList();

                foreach (var piece in squares)
                {
                    if (piece.ChessPiece_ != null &&
                        piece.ChessPiece_ is ChessPiece chessPiece &&
                        chessPiece.PieceColor == king.PieceColor &&
                        chessPiece is IChessMoves chessMoves &&
                        !string.IsNullOrWhiteSpace(piece.ChessPiece_.PieceImage))
                    {
                        // Use helper to run simulation on clone and receive filtered lists without mutating live board
                        var (availableMoves, attackMoves, specialMoves) = CheckMoveHelper.GetFilteredMoves(chessMoves, clonedTarget, pathToKing, clonedBoard);

                        var filteredAvailable = availableMoves;
                        var filteredAttack = attackMoves;
                        var filteredSpecial = specialMoves;

                        if (filteredAvailable == null && filteredAttack == null)
                            continue;

                        // Find corresponding original square on the live board by location
                        var loc = piece.BoardLocation;
                        var origSquare = GameControls.ChessBoard.ChessSquares_?.FirstOrDefault(s => s.BoardLocation == loc);

                        if (origSquare?.ChessPiece_ != null)
                        {
                            // Assign back on UI thread to avoid cross-thread collection exceptions
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                // perform assignments under shared lock to avoid concurrent readers/writers
                                lock (SyncRoot.MovesLock)
                                {
                                    if (origSquare.ChessPiece_ is IChessMoves origMoves && filteredAvailable != null)
                                    {
                                        origMoves.AvailableMoves = filteredAvailable;
                                    }

                                    if (origSquare.ChessPiece_ is IChessMoves origAttackMoves && filteredAttack != null)
                                    {
                                        origAttackMoves.AttackMoves = filteredAttack;
                                    }

                                    if (origSquare.ChessPiece_ is IPieceSpecialMove origSpecial && filteredSpecial != null)
                                    {
                                        origSpecial.SpecialMoves = filteredSpecial;
                                    }
                                }
                            });
                        }
                    }
                }
            }
        }

        #endregion

        #region -- Movement Methods --

        /// <summary>
        /// Performs the movement of a chess piece from its current square to a chosen square on the chessboard.
        /// </summary>
        /// <param name="chessPiece">The chess piece to move.</param>
        /// <param name="ChoosenSquare">The square chosen by the player.</param>
        /// <param name="chessSquare">The current square of the chess piece.</param>
        /// <param name="chessBoard">The chess board on which the move is performed.</param>
        /// <param name="promotionChessPiece">The chess piece to promote to, if applicable.</param>
        public void PerfomChessPieceMove(ChessPiece? chessPiece, ChessSquare ChoosenSquare,
            ChessSquare chessSquare, Board chessBoard, ChessPiece? promotionChessPiece = null)
        {
            if (ChoosenSquare?.ChessPiece_ is ChessPiece selectedChessPiece && chessPiece != null)
            {
                // Determine which chess piece to place on the chosen square: either the promotion piece or the original piece
                ChessPiece chessPieceToPlace = (promotionChessPiece is not null) ? promotionChessPiece : chessPiece;

                // If the chess piece being moved is a pawn, create a deep clone of it and update its location and moved status.
                if (chessPiece is Pawn pawn)
                {
                    ClonePiece = (Pawn)pawn.DeepClone();

                    ClonePiece.CurrentLocation = new ChessSquareLocation(
                        chessSquare.BoardLocation.X,
                        chessSquare.BoardLocation.Y
                    );

                    ClonePiece.ChessPieceMoved = true;
                }

                // Play sound effects based on whether the move is a capture or a regular move, if sound effects are enabled in settings.
                if (GameControls.AppSettingService.Settings.IsEnableSoundEffects)
                {
                    if (chessSquare.ChessPiece_ is not null)
                        ChessSoundPlayer.PlayerCapture();
                    else
                        ChessSoundPlayer.PlayMove();
                }

                // Use the UI thread to animate the movement of the chess piece and update the board state atomically.
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // animate chess piece selected by player and place the chess piece on the correct square
                    // Use a lambda that clears the source square and places the piece atomically to avoid
                    // rendering an intermediate empty board state during animation.
                    OnPieceMoveAnimated?.Invoke(
                        () =>
                        {
                            // clear source and place piece together on the UI thread
                            PlacePiece(chessPieceToPlace, chessSquare, chessBoard);
                        },
                        chessPiece.CurrentLocation,
                        chessSquare.BoardLocation, chessPiece.PieceImage);

                    ChoosenSquare.ChessPiece_ = null;
                });
            }
        }

        /// <summary>
        /// Performs a castling move involving a king and a rook on the chessboard. This method updates the positions of the king and rook, marks them as moved, 
        /// and removes the castling move from the list of available castling moves. 
        /// It also triggers an animation for the castling move if sound effects are enabled in the application settings.
        /// </summary>
        /// <param name="choosenChessPiece">The chess piece chosen by the player to perform the castling move.</param>
        /// <param name="moveSelected">The selected move representing the castling move.</param>
        /// <param name="castlingMoves">The list of available castling moves.</param>
        /// <param name="chessBoard">The chess board on which the move is performed.</param>
        public void PerfromCastlingMove(ChessPiece choosenChessPiece, MovesAvailable moveSelected,
         List<MovesAvailable> castlingMoves, ObservableCollection<ChessSquare> chessBoard)
        {
            // Cast the king and rook involved in the castling move
            var king = Castling.CastKing(choosenChessPiece, moveSelected.ChessPieceRef);
            var rook = Castling.CastRook(choosenChessPiece, moveSelected.ChessPieceRef);

            if (king != null &&
                rook != null &&
                moveSelected?.SquareOne != null &&
                moveSelected?.SquareTwo != null)
            {
                // Get the squares corresponding to the king and rook's current locations on the chessboard
                var king_ = king.CurrentLocation.GetSquare(chessBoard);
                var rook_ = rook.CurrentLocation.GetSquare(chessBoard);

                if(GameControls.AppSettingService.Settings.IsEnableSoundEffects)
                   ChessSoundPlayer.PlayMove();

                // Use the UI thread to animate the castling move and update the board state atomically.
                Application.Current.Dispatcher.InvokeAsync(async () =>
                {

                    // Animate the movement of the king and rook to their new positions on the chessboard
                    OnPiecesAnimateCasltingMove?.Invoke(
                   () => PlacePiece(king, moveSelected.SquareOne, GameControls.ChessBoard),
                   () => PlacePiece(rook, moveSelected.SquareTwo, GameControls.ChessBoard),
                   (king.CurrentLocation, rook.CurrentLocation),
                   (moveSelected.SquareOne.BoardLocation, moveSelected.SquareTwo.BoardLocation),
                   (king.PieceImage, rook.PieceImage));

                });

                // Mark the king and rook as having moved to prevent future castling moves
                var chessPiece1 = (IMoved)king;
                var chessPiece2 = (IMoved)rook;

                // Mark the king and rook as having moved to prevent future castling moves
                chessPiece1.ChessPieceMoved = true;
                chessPiece2.ChessPieceMoved = true;

                if (king_ != null && rook_ != null)
                {
                   king_.ChessPiece_ = null;
                   rook_.ChessPiece_ = null;
                }

                // Remove the castling move from the list of available castling moves to prevent it from being used again
                castlingMoves.Remove(moveSelected);
            }
        }

        /// <summary>
        /// Places a chess piece on a specified chess square on the chessboard. This method updates the current location of the chess piece, assigns it to the specified square, and generates the available moves for the piece.
        /// </summary>
        /// <param name="chessPiece">The chess piece to be placed on the board.</param>
        /// <param name="chessSquare">The chess square where the piece will be placed.</param>
        /// <param name="chessBoard">The chessboard on which the piece and square exist.</param>
        public static void PlacePiece(ChessPiece chessPiece, ChessSquare chessSquare, Board chessBoard)
        {
            if (chessBoard.ChessSquares_ is not null)
            {
                chessPiece.CurrentLocation = new ChessSquareLocation(
                    chessSquare.BoardLocation.X,
                    chessSquare.BoardLocation.Y
                );

                // Assign the chess piece to the specified square on the chessboard
                chessSquare.ChessPiece_ = chessPiece;

                // Generate the available moves for the chess piece based on its type and position on the board
                if (chessSquare.ChessPiece_ is IChessMoves chessMoves)
                {
                    if (chessSquare.ChessPiece_ is King)
                    {
                        chessMoves.GenerateAvailableMoves(chessBoard);
                        chessMoves.GenerateAttackMoves(chessBoard);
                    }
                    else
                    {
                        chessMoves.GenerateValidMoves(chessBoard);
                    }
                }
            }
        }

        #endregion
    }
}
