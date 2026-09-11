using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Interfaces.Services;
using ChessGame.Square;
using System.Collections.ObjectModel;


namespace ChessGame.Controls
{
    /// <summary>
    /// Represents the player controls in a chess game, providing methods to handle player interactions with the chessboard, such as selecting pieces, validating moves, 
    /// and performing special moves like castling and en passant captures.
    /// </summary>
    public class PlayerControls : GameControls
    {
        #region -- - Properties ---
        private GameControls GameControls { get; set; } = default!;

        public bool IsDevelopmentMode { get; set; }

        #endregion

        #region -- Constructors ---
        public PlayerControls() { }

        public PlayerControls(GameControls gameControls)
        {
            GameControls = gameControls;

        }

        #endregion

        #region -- Player Move Validation Methods --
        /// <summary>
        /// Checks if the selected chess piece belongs to the player based on the piece's color and the player's assigned color.
        /// </summary>
        /// <param name="chessSquare">The chess square to check.</param>
        /// <returns>True if the chess piece belongs to the player; otherwise, false.</returns>
        public bool IsPlayerChessPieceSelected(ChessSquare? chessSquare)
        {
            return (!string.IsNullOrWhiteSpace(chessSquare?.ChessPiece_?.PieceImage)
                  && (chessSquare.ChessPiece_.PieceColor == GameControls.ChessGameService?.playerColor));
            
        }

        /// <summary>
        /// Performs a castling move for the player based on the chosen chess square, the next chess square, and the current state of the chessboard.
        /// </summary>
        /// <param name="choosenSquare">The first chess square involved in the castling move.</param>
        /// <param name="chessSquare">The second chess square involved in the castling move.</param>
        /// <param name="chessBoard">The current state of the chessboard.</param>
        public void PlayerCastlingMove(ChessSquare choosenSquare, ChessSquare? chessSquare, Board chessBoard)
        {
            IPieceSpecialMove castlingChessPiece;

            // Check if the chosen square's chess piece implements IPieceSpecialMove and if the next square is not null
            if (choosenSquare?.ChessPiece_ is IPieceSpecialMove c && chessSquare != null)
                castlingChessPiece = c;
            else
                return;

            // Find the special move corresponding to the next chess square's location in the castling chess piece's special moves list
            var move = castlingChessPiece?.SpecialMoves?.FirstOrDefault(m => m.Move.Equals(chessSquare.BoardLocation));

            if (move != null &&
                chessBoard.ChessSquares_ != null &&
                castlingChessPiece?.SpecialMoves != null &&
                choosenSquare.ChessPiece_ != null)
            {
                // Perform the castling move using the GameControls.MovementControls.PerfromCastlingMove method
                GameControls.MovementControls.PerfromCastlingMove(choosenSquare.ChessPiece_, move,
                                castlingChessPiece.SpecialMoves, chessBoard.ChessSquares_);
            }
        }

        /// <summary>
        /// Checks if the player's selected move is an available move and outputs the corresponding available move if found.
        /// </summary>
        /// <param name="playerChessPiece">The player's chess piece.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <param name="availableMove">The available move if found.</param>
        /// <returns>True if the move is available; otherwise, false.</returns>
        public static bool IsPlayerAvailableMove(IChessMoves? playerChessPiece, ChessSquare? nextChessSquare, out MovesAvailable? availableMove)
        {
            availableMove = null;

            // Check if the player's chess piece and the next chess square are not null, and if the player's available moves contain the next chess square's location
            return (playerChessPiece != null &&
                           playerChessPiece.AvailableMoves != null &&
                           nextChessSquare != null &&
                           MovementControls.ContainsMove(playerChessPiece.AvailableMoves, nextChessSquare.BoardLocation, out availableMove));
        }

        /// <summary>
        /// Checks if the player's selected move is an available move without outputting the corresponding available move.
        /// </summary>
        /// <param name="playerChessPiece">The player's chess piece.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <returns>True if the move is available; otherwise, false.</returns>
        public static bool IsPlayerAvailableMove(IChessMoves? playerChessPiece, ChessSquare? nextChessSquare)
        {
             return (playerChessPiece != null &&
                            playerChessPiece.AvailableMoves != null &&
                            nextChessSquare != null &&
                            MovementControls.ContainsMove(playerChessPiece.AvailableMoves, nextChessSquare.BoardLocation));
        }

        /// <summary>
        /// Checks if the player's selected move is an attack move without outputting the corresponding available move.
        /// </summary>
        /// <param name="playerChessPiece">The player's chess piece.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <returns>True if the move is an attack move; otherwise, false.</returns>
        public static bool IsPlayerAttackMove(IChessMoves? playerChessPiece, ChessSquare? nextChessSquare)
        {
            return (playerChessPiece != null &&
                        playerChessPiece.AttackMoves != null &&
                        nextChessSquare != null &&
                        MovementControls.ContainsMove(playerChessPiece.AttackMoves, nextChessSquare.BoardLocation));
        }

        /// <summary>
        /// Checks if the player's selected move is an attack move and outputs the corresponding available move if found.
        /// </summary>
        /// <param name="playerChessPiece">The player's chess piece.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <param name="attackMove">The available attack move if found.</param>
        /// <returns>True if the move is an attack move; otherwise, false.</returns>
        public static bool IsPlayerAttackMove(IChessMoves? playerChessPiece, ChessSquare? nextChessSquare, out MovesAvailable? attackMove)
        {
            attackMove = null;

            return (playerChessPiece != null &&
                        playerChessPiece.AttackMoves != null &&
                        nextChessSquare != null &&
                        MovementControls.ContainsMove(playerChessPiece.AttackMoves, nextChessSquare.BoardLocation, out attackMove));
        }


        /// <summary>
        /// Checks if the player's selected move is a special move without outputting the corresponding available move.
        /// </summary>
        /// <param name="playerChessPiece">The player's chess piece.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <returns>True if the move is a special move; otherwise, false.</returns>
        public static bool IsPlayerSpecialMove(IChessMoves? playerChessPiece, ChessSquare? nextChessSquare)
        {
            return playerChessPiece != null &&
                        playerChessPiece is IPieceSpecialMove pieceSpecialMove &&
                        pieceSpecialMove.SpecialMoves != null &&
                        nextChessSquare != null &&
                        MovementControls.ContainsMove(pieceSpecialMove.SpecialMoves, nextChessSquare.BoardLocation);
        }

        /// <summary>
        /// Checks if the player's selected move is a special move and outputs the corresponding available move if found.
        /// </summary>
        /// <param name="playerChessPiece">The player's chess piece.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <param name="specialMove">The available special move if found.</param>
        /// <returns>True if the move is a special move; otherwise, false.</returns>
        public static bool IsPlayerSpecialMove(IChessMoves? playerChessPiece, ChessSquare? nextChessSquare, out MovesAvailable? specialMove)
        {
            specialMove = null;

            return playerChessPiece != null &&
                        playerChessPiece is IPieceSpecialMove pieceSpecialMove &&
                        pieceSpecialMove.SpecialMoves != null &&
                        nextChessSquare != null &&
                        MovementControls.ContainsMove(pieceSpecialMove.SpecialMoves, nextChessSquare.BoardLocation, out specialMove);
        }

        #endregion

        #region -- Castlting Move Validation Methods --

        /// <summary>
        /// Checks if two chess pieces of the same color have been selected.
        /// </summary>
        /// <param name="choosenSquare">The first chess square to check.</param>
        /// <param name="nextChessSquare">The second chess square to check.</param>
        /// <returns>True if both chess squares have chess pieces of the same color; otherwise, false. </returns>
        public static bool TwoPlayerChessPiecesSelected(ChessSquare choosenSquare, ChessSquare? nextChessSquare)
        {
            if (nextChessSquare == null) return false;

            return choosenSquare?.ChessPiece_ != null &&
                        nextChessSquare.ChessPiece_ != null;
        }

        /// <summary>
        /// Checks if the player has selected a castling move based on the chosen chess square and the next chess square.
        /// </summary>
        /// <param name="choosenSquare">The first chess square to check.</param>
        /// <param name="nextChessSquare">The second chess square to check.</param>
        /// <returns>True if the player has selected a castling move; otherwise, false.</returns>
        public static bool PlayerSelectedCastlingMove(ChessSquare choosenSquare, ChessSquare? nextChessSquare)
        {
            if (choosenSquare?.ChessPiece_ == null ||
                nextChessSquare?.ChessPiece_ == null) return false;

            // Check if the chosen square's chess piece implements ICastlingMove and IPieceSpecialMove, and if the next square's location is in the special moves list
            return choosenSquare.ChessPiece_ is ICastlingMove &&
                        choosenSquare.ChessPiece_ is IPieceSpecialMove castlingChessPiece &&
                        castlingChessPiece.SpecialMoves != null &&
                        MovementControls.ContainsMove(castlingChessPiece.SpecialMoves, nextChessSquare.BoardLocation);
        }

        #endregion

        #region -- En Passant Move Validation Methods --

        /// <summary>
        /// Checks if the move is an enPassant capture move.
        /// </summary>
        /// <param name="choosenSquare">The chosen chess square.</param>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <param name="chessBoard">The chess board.</param>
        /// <returns>True if the move is an enPassent capture move; otherwise, false.</returns>
        public bool IsEnPassentCaptureMove(ChessSquare choosenSquare, ChessSquare nextChessSquare, Board chessBoard)
        {
            return choosenSquare?.ChessPiece_ != null &&
                            SquareIsEnPassentAttack(choosenSquare, nextChessSquare, chessBoard.ChessSquares_);
        }

        /// <summary>
        /// Checks if the selected square is an en passant attack square based on the chess piece's square, the selected square, and the current state of the chessboard.
        /// </summary>
        /// <param name="chessPieceSquare">The chess piece's square.</param>
        /// <param name="selectedSquare">The selected square.</param>
        /// <param name="chessBoard">The current state of the chessboard.</param>
        /// <returns>True if the selected square is an en passant attack square; otherwise, false.</returns>
        public bool SquareIsEnPassentAttack(ChessSquare? chessPieceSquare, ChessSquare? selectedSquare,
            ObservableCollection<ChessSquare>? chessBoard)
        {
            if (chessBoard != null)
            {
                foreach (var square in chessBoard)
                {
                    if (square.ChessPiece_ != null &&
                        chessPieceSquare != null &&
                        selectedSquare != null &&
                        square.ChessPiece_ is Pawn &&
                        chessPieceSquare.ChessPiece_ is Pawn playerPawn &&
                        GameControls.SpecialMoveHandler.EnPassant.EnPassentAttackSquare.Equals(selectedSquare.BoardLocation) &&
                        playerPawn.CanCaptureEnpassant)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the pawn that was captured in an en passant move from the chessboard and updates the removed pieces collection based on the player's color.
        /// </summary>
        /// <param name="chessBoard">The chess board.</param>
        /// <param name="playerColor">The color of the player.</param>
        /// <param name="chessGameService">The chess game service.</param>
        public static void RemovePawnInEnPassentMove(Board chessBoard, ChessPieceColors playerColor, IIChessGameService chessGameService)
        {
           if(chessBoard.EnPassantTarget.Pawn != null && chessBoard.EnPassantTarget.Pawn is Pawn pawn)
            {
                // Get the square of the pawn that was attacked in the en passant move
                var pawnAttackedInEnPassant = chessBoard.EnPassantTarget.Pawn.CurrentLocation.GetSquare(chessBoard.ChessSquares_ ?? []);

                if (pawnAttackedInEnPassant != null)
                {
                    // Check if the pawn that was attacked in the en passant move is the same as the pawn that was moved
                    if (pawnAttackedInEnPassant.ChessPiece_ is Pawn foundPawn && pawn.CurrentLocation == foundPawn.CurrentLocation)
                    {
                        // Remove the pawn from the chessboard and update the removed pieces collection based on the player's color
                        if (chessGameService.playerColor == playerColor)
                        {
                            chessBoard?.RemovedOpponentChessPieces?.Add(foundPawn.PieceImage);
                        }
                        else
                        {
                            chessBoard?.RemovedPlayerChessPieces?.Add(foundPawn.PieceImage);
                        }

                        pawnAttackedInEnPassant.ChessPiece_ = null;
                    }
                } 
            }
        }

        #endregion

        #region -- Helper Methods --

        /// <summary>
        /// Checks if the chess square is either empty or contains an opponent's chess piece based on the piece's color and the opponent's assigned color.
        /// </summary>
        /// <param name="chessSquare">The chess square to check.</param>
        /// <returns>True if the chess square is either empty or contains an opponent's chess piece; otherwise, false.</returns>
        public bool IsOpponetChessPieceOrEmptySquare(ChessSquare? chessSquare)
        {
            return ((chessSquare?.ChessPiece_?.PieceColor == GameControls.ChessGameService?.opponetColor)
                            || (string.IsNullOrWhiteSpace(chessSquare?.ChessPiece_?.PieceImage)));
        }

        /// <summary>
        /// Checks if the next chess square contains an opponent's chess piece, indicating a capture move.
        /// </summary>
        /// <param name="nextChessSquare">The next chess square to move to.</param>
        /// <returns>True if the next chess square contains an opponent's chess piece; otherwise, false.</returns>
        public static bool IsCaptureMove(ChessSquare nextChessSquare)
        {
            return !string.IsNullOrWhiteSpace(nextChessSquare?.ChessPiece_?.PieceImage);
        }

        #endregion
    }
}
