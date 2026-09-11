using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessPieces;
using ChessGame.Controls;
using ChessGame.Enums;
using ChessGame.Game.MoveQueue;
using ChessGame.Interfaces.Game;
using ChessGame.Square;
using ChessGame.Strategies.Tools.StockfishTools;
using ChessGame.Utilities;
using System.Diagnostics;

namespace ChessGame.ChessPlayers
{
    /// <summary>
    /// Represents a player in a chess game, managing the player's moves, selected squares, and interactions with the game controls.
    /// </summary>
    public class Player : ObservableObject
    {
        #region -- Properties --
        public GameControls GameControls { get; set; } = default!;
        public IsCheckControls IsCheckControls { get; set; } = default!;
        private bool IsPlayerMove_ { get; set; }
        private ChessPiece? chessPiece;
        private ChessSquare? choosenSquare;
        public ChessSquare ChessSquare { get; set; } = null!;

        public bool IsPlayerMove
        {
            get => IsPlayerMove_;
            set
            {
                IsPlayerMove_ = value;
                OnPropertyChanged(nameof(IsPlayerMove));
            }
        }

        public ChessSquare? ChoosenSquare
        {
            get => choosenSquare;
            set
            {
                choosenSquare = value;
                OnPropertyChanged(nameof(ChoosenSquare));
            }
        }

        public ChessPiece? ChessPiece
        {
            get => chessPiece;
            set
            {
                chessPiece = value;
                OnPropertyChanged(nameof(ChessPiece));
            }
        }

        public string GetParamName()
        {
            return $"{nameof(GameControls.ChessGameService)} is null or {nameof(GameControls.ChessGameService.selectedGameMode)} is null";
        }

        #endregion

        #region -- Constuctor --

        public Player(GameControls gameControls)
        {
            GameControls = gameControls;
            IsCheckControls = new IsCheckControls(GameControls);

            // game variables
            IsPlayerMove = false;
            ChoosenSquare = null!;
        }



        #endregion

        #region -- Player Move Method ---

        /// <summary>
        /// Handles the player's move in the chess game, determining the type of move made and updating the game state accordingly.
        /// </summary>
        /// <param name="chessSquare">The chess square selected by the player.</param>
        /// <param name="playerColor">The color of the current player.</param>
        /// <param name="paramName">A string containing an error message.</param>
        /// <returns>A tuple containing move information used in a chess game.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<(ChessMoveInfo? MoveInfo, StockChessMoveInfo? SfMoveInfo)> PlayerMove(
            ChessSquare? chessSquare, ChessPieceColors playerColor, string paramName
            )
        {

            if (GameControls.ChessBoard?.ChessSquares_ == null)
                throw new InvalidOperationException("Player must select a move!");

            try
            {
                if (GameControls.ChessGameService is null || GameControls.ChessGameService.selectedLevel is null)
                    throw new ArgumentNullException(paramName);

                // stores the move location of the chess piece on the chess board
                string moveChessBoardLocation = string.Empty;
                // stores the type of move made by the player
                MoveType moveType = MoveType.NORMAL;

                // check if the player selected a chess square that contains its own chess piece
                if (GameControls.PlayerControls.IsPlayerChessPieceSelected(chessSquare))
                {
                    // check if the player has already selected a chess square and is now selecting a second chess square
                    if (ChoosenSquare != null &&
                        PlayerControls.TwoPlayerChessPiecesSelected(ChoosenSquare, chessSquare) &&
                        PlayerControls.PlayerSelectedCastlingMove(ChoosenSquare, chessSquare)) // player selected a castling move
                    {
                        // perform the castling move
                        GameControls.PlayerControls.PlayerCastlingMove(ChoosenSquare, chessSquare, GameControls.ChessBoard);
                        this.IsPlayerMove = false;

                        // concats the first selected chess squares' file and rank with the second chess squares' file and rank
                        moveChessBoardLocation = new string(ChoosenSquare.ChessBoardLocation + chessSquare?.ChessBoardLocation);

                        var pieceType = (chessSquare?.ChessPiece_?.PieceType is PieceType.ROOK) ? PieceType.KING : PieceType.ROOK;

                        // return move type information
                        return (new ChessMoveInfo(playerColor, pieceType , moveChessBoardLocation, MoveType.CASTLING), null);
                    }
                    else
                    {
                        // player's first selected chess square
                        ChoosenSquare = chessSquare;

                        // enable indicators on the selected chess square to show possible moves for the selected chess piece
                        if (ChoosenSquare != null)
                          GameControls.SettingControls.IsEnableIndicators(ChoosenSquare, GameControls.ChessBoard);

                        // indicates that the player has selected a chess square and is now ready to select a second chess square
                        this.IsPlayerMove = true;

                        return (null, null);
                    }

                }
                // check if the player selected a chess square that contains an opponent's chess piece or an empty square
                else if (GameControls.PlayerControls.IsOpponetChessPieceOrEmptySquare(chessSquare) && IsPlayerMove)
                {
                    // safe-fall
                    ArgumentNullException.ThrowIfNull(chessSquare);
                    
                    // get file and rank of both the first and second selected chess squares
                    var move = new string(ChoosenSquare?.ChessBoardLocation + chessSquare.ChessBoardLocation);

                    // store the second selected chess square
                    ChessSquare = chessSquare;
                    // store the chess piece of the first selected chess square
                    var chessPiece = ChoosenSquare?.ChessPiece_;
                    var playerChessPiece = (IChessMoves?)ChoosenSquare?.ChessPiece_;


                    // check if the selected move is a valid move for the selected chess piece
                    if (PlayerControls.IsPlayerAvailableMove(playerChessPiece, chessSquare, out MovesAvailable? typeOfMove) ||
                        PlayerControls.IsPlayerAttackMove(playerChessPiece, chessSquare, out typeOfMove) ||
                        PlayerControls.IsPlayerSpecialMove(playerChessPiece, chessSquare, out typeOfMove))
                    {

                        // check if the selected move is a capture move and if the second selected chess square contains an opponent's chess piece
                        if (PlayerControls.IsCaptureMove(chessSquare) && chessSquare.ChessPiece_ != null)

                        {
                            // store (X, y) coordinates of the second chess square's chess piece 
                            var chessPieceLocation = chessSquare.ChessPiece_.CurrentLocation;

                            // add the captured chess pieces to a list
                            GameControls.ChessBoard.RemovedOpponentChessPieces?.Add(chessSquare.ChessPiece_.PieceImage);

                            // remove the captured chess piece's from a list of tracked chess pieces on the chess board
                            GameControls.ChessBoard.RemoveChessPiece(colorToRemove: (playerColor is ChessPieceColors.WHITE) 
                                ? ChessPieceColors.BLACK : ChessPieceColors.WHITE, chessPieceToRemove: chessPieceLocation);

                            // captured chess piece is pawn type
                            if (chessSquare.ChessPiece_ is Pawn pawn && 
                                GameControls.SpecialMoveHandler?.PawnChessPieces.ContainsKey(pawn.PieceColor) == true)
                            {
                                // remove pawn from a list of watched pawns
                                GameControls.SpecialMoveHandler?.PawnChessPieces[pawn.PieceColor].Remove(pawn.PawnIndex);
                                
                            }
                            // captured chess piece is rook type
                            else if(chessSquare.ChessPiece_ is Rook rook && 
                                GameControls.SpecialMoveHandler?.CastlingChessPieces.ContainsKey(rook.PieceColor) == true)
                            {
                                // remove rook from a list of watched rooks and kings
                                GameControls.SpecialMoveHandler?.CastlingChessPieces[rook.PieceColor].Remove(rook.CastleSide);
                            }
                        }
                        // check if the selected move is an en passant capture move
                        else if (ChoosenSquare != null &&
                            GameControls.PlayerControls.IsEnPassentCaptureMove(ChoosenSquare, ChessSquare, GameControls.ChessBoard) && 
                            ChoosenSquare != null && 
                            ChessSquare != null &&
                            GameControls.ChessBoard.EnPassantTarget.Pawn != null)
                        {
                            // store the (X,Y) coordinates of the pawn being captured during an en passant move
                            var chessPieceLocation = GameControls.ChessBoard.EnPassantTarget.Pawn.CurrentLocation;

                            // run on UI thread to remove the pawn from the chess piece
                            _ = await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                            {
                                PlayerControls.RemovePawnInEnPassentMove(GameControls.ChessBoard, playerColor, GameControls.ChessGameService);
                            });

                            // remove pawn from a list of watched chess pieces on the chess board
                            GameControls.ChessBoard.RemoveChessPiece(colorToRemove: (playerColor is ChessPieceColors.WHITE)
                                ? ChessPieceColors.BLACK : ChessPieceColors.WHITE, chessPieceToRemove: chessPieceLocation);

                            // captured chess piece is pawn type
                            if (chessSquare.ChessPiece_ is Pawn pawn &&
                                GameControls.SpecialMoveHandler?.PawnChessPieces.ContainsKey(pawn.PieceColor) == true)
                            {
                                // remove pawn from a list of watched pawns
                                GameControls.SpecialMoveHandler?.PawnChessPieces[pawn.PieceColor].Remove(pawn.PawnIndex);
                            }
                        }

                        // check for nulls (defensive-check)
                        if (chessPiece != null &&
                            ChessSquare != null &&
                            ChoosenSquare != null)
                        {
                            // temp chess piece
                            this.ChessPiece = chessPiece;

                            // performs the chess piece movement
                            GameControls.MovementControls.PerfomChessPieceMove(chessPiece, ChoosenSquare, chessSquare, GameControls.ChessBoard);

                        }

                        // player turn is over
                        this.ChessPiece = null;
                        ChoosenSquare = null;
                        IsPlayerMove = false;

                        // determines move type
                        if (typeOfMove is not null)
                            moveType = typeOfMove.MoveType;

                        // check for nulls
                        if(chessPiece?.PieceType is null)
                            throw new ArgumentNullException(paramName);

                        // return move information
                        return (new ChessMoveInfo(playerColor, chessPiece.PieceType,  move, moveType), 
                            new StockChessMoveInfo(chessSquare.ChessPiece_?.PieceType, moveType));

                    }
                    else
                    {
                        ChoosenSquare = null;
                        IsPlayerMove = false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"ERORR!!! --- {e.Message}");
            }

            return (null, null);
        }

        #endregion
    }
}
