using ChessGame.ChessBoard;
using ChessGame.ChessGameMoves.Moves;
using ChessGame.ChessPieces;
using ChessGame.ChessPieces.Extenstions;
using ChessGame.Enums;
using ChessGame.Interfaces.Game;
using ChessGame.Structs;
using ChessGame.Utilities;

namespace ChessGame.Controls
{
    /// <summary>
    /// The IsCheckControls class is responsible for managing the state of check and checkmate in a chess game. 
    /// It provides methods to determine if a player's king is in check, if the check can be blocked, and if the player is in checkmate. 
    /// The class maintains references to the current game controls, the king being checked, and whether the player is currently in check. 
    /// It also includes methods to reset the check state and to place chess pieces on the board for simulation purposes.
    /// </summary>
    public class IsCheckControls : ObservableObject
    {
        #region -- Properties --
        private GameControls GameControls { get; set; } = default!;
        private King? IsKing { get; set; }
        private bool IsPlayerCheck_ { get; set; }

        public King? King 
        {   get => IsKing;
            set
            {
                IsKing = value;
                OnPropertyChanged(nameof(King));
            }
        }

        public bool IsPlayerCheck 
        {
            get => IsPlayerCheck_;
            set
            {
                IsPlayerCheck_ = value;
                OnPropertyChanged(nameof(IsPlayerCheck));
            }
        }

        #endregion

        #region -- Constructors --
        public IsCheckControls() { }

        public IsCheckControls(GameControls gameControls)
        {
            IsPlayerCheck_ = false;
            GameControls = gameControls;
        }

        #endregion

        #region -- Check and Checkmate Methods --

        /// <summary>
        /// Determines if the player's king is in check by checking if any of the opponent's chess pieces have available moves that can attack the king's current location.
        /// </summary>
        /// <param name="playerColor">The color of the player's pieces.</param>
        /// <param name="king">The player's king piece.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>A tuple containing a boolean indicating if the king is in check and a list of opponent chess pieces that are attacking the king.</returns>
        public (bool IsCheck, List<ChessPiece> TargetChessPieces) IsCheck(ChessPieceColors playerColor, King king, Board chessBoard)
        {
            if (chessBoard != null && chessBoard.ChessSquares_ != null)
            {
                List<ChessPiece> attackers = [];

                // Snapshot the collection to avoid collection-modified exceptions when other threads mutate it
                var squares = chessBoard.ChessSquares_?.ToList();

                foreach (var square in squares ?? [])
                {
                    if (square.ChessPiece_ is ChessPiece attacker
                        && attacker.PieceColor != playerColor)
                    {
                        if (attacker is IChessMoves attackerChessPiece &&
                            attackerChessPiece.AvailableMoves is not null)
                        {
                            List<MovesAvailable> chessPieceOpponetMoves = [];

                            // If the attacker is a pawn, use its attack moves; otherwise, use its available moves.
                            if (attackerChessPiece is Pawn pawn && pawn.AttackMoves != null)
                                chessPieceOpponetMoves = pawn.AttackMoves;
                            else
                                chessPieceOpponetMoves = attackerChessPiece.AvailableMoves;

                            // Check if any of the opponent's moves can attack the king's current location.
                            var attacksKingCurrentLocation = chessPieceOpponetMoves.Any(oppMove => oppMove.Move == king.CurrentLocation);

                            // If the opponent's move can attack the king's current location, add the attacker to the list of attackers.
                            if (attacksKingCurrentLocation)
                            {
                                // Create a deep copy of the attacker to avoid mutating the original piece during further processing.
                                var cloneChessPiece = ChessPieceExtenstions.CloneChessPieceOnType(attacker);
                                ArgumentNullException.ThrowIfNull(cloneChessPiece);

                                // Add the cloned attacker to the list of attackers.
                                attackers.Add(cloneChessPiece);
                            }
                        }
                    }
                }

                // If there are any attackers, set the king's check status and return true along with the list of attackers.
                if (attackers?.Count > 0)
                {
                    King = king;
                    king.IsChecked = true;
                    IsPlayerCheck = true;
                    return (true, attackers);
                }

            }

            // If there are no attackers, reset the king's check status and return false along with an empty list of attackers.
            IsPlayerCheck = false;
            king.IsChecked = false;
            return (false, new List<ChessPiece>());
        }

        /// <summary>
        /// Determines if the player is in checkmate by checking if the king has any available moves and 
        /// if any of the player's chess pieces can block the check from the attacking pieces.
        /// </summary>
        /// <param name="playerColor">The color of the player to check for checkmate.</param>
        /// <param name="king">The king piece to check for checkmate.</param>
        /// <param name="targetChessPieces">The list of attacking chess pieces targeting the king.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>True if the player is in checkmate; otherwise, false.</returns>
        public bool IsCheckMate(ChessPieceColors playerColor, King king, List<ChessPiece> targetChessPieces, Board chessBoard)
        {
            List<(ChessPiece Target, bool Checkmate)> IsCheckmate = [];

            if (chessBoard != null && chessBoard.ChessSquares_ != null && king.AvailableMoves != null)
            {
                // Check if the king has any available moves. If it does, the player is not in checkmate.
                if (king.AvailableMoves.Count != 0)
                    return false;

                // Get the player's chess pieces based on the player's color.
                var playerChessPieces = (playerColor is ChessPieceColors.WHITE) ? chessBoard.WhiteChessPieces : chessBoard.BlackChessPieces;

                foreach (var targetChessPiece in targetChessPieces)
                {
                    // Initialize a tuple to track the target chess piece and whether it results in checkmate.
                    (ChessPiece target, bool checkmate) targetCheckmate = (targetChessPiece, true);

                    foreach (var cp in playerChessPieces)
                    {
                        if (cp is ChessPiece teamChessPiece
                            && teamChessPiece is IChessMoves teamMateMoves
                            && teamMateMoves is not null
                            && teamMateMoves.AvailableMoves != null 
                            && teamMateMoves.AttackMoves != null)
                        {
                            foreach (var move in teamMateMoves?.AttackMoves ?? [])
                            {
                                // Check if any of the player's chess pieces can capture the attacking piece. If so, checkmate is false.
                                if (targetChessPiece != null && move.Move == targetChessPiece.CurrentLocation)
                                {
                                    // If the player's chess piece can capture the attacking piece, then it's not a checkmate.
                                    targetCheckmate.checkmate = false;
                                    break;
                                }
                            }

                            // If the target chess piece is still considered a checkmate threat,
                            // check if the check can be blocked by any of the player's chess pieces.
                            if (targetCheckmate.checkmate is true && targetChessPiece != null)
                            {
                                // Determine if the check can be blocked by any of the player's chess pieces.
                                var IsBlockable = IsCheckBlockable(targetChessPiece, teamChessPiece, king, chessBoard);

                                // Update the checkmate status based on whether the check can be blocked.
                                targetCheckmate.checkmate = !IsBlockable;

                                // If the check can be blocked, we can break out of the loop early since we already know it's not a checkmate.
                                if (IsBlockable)
                                {
                                    break;
                                }
                                
                            }
                        }

                        // If the target chess piece is no longer considered a checkmate threat, we can break out of the loop early.
                        if (targetCheckmate.checkmate is false)
                            break;
                    }

                    // Add the result for this target chess piece to the list of checkmate evaluations.
                    IsCheckmate.Add(targetCheckmate);

                }
            }

            // If any of the target chess pieces are still considered a checkmate threat, then the player is in checkmate.
            if (IsCheckmate.Any(cm => cm.Checkmate is true))
            {
                King = null;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the check on the player's king can be blocked by any of the player's chess pieces.
        /// </summary>
        /// <param name="attacker">The attacking chess piece putting the king in check.</param>
        /// <param name="teamPiece">The player's chess piece that may block the check.</param>
        /// <param name="king">The player's king that is in check.</param>
        /// <param name="chessBoard">The current state of the chess board.</param>
        /// <returns>True if the check can be blocked by the player's chess piece; otherwise, false.</returns>
        public bool IsCheckBlockable(ChessPiece attacker, ChessPiece teamPiece, King king, Board chessBoard)
        {
            var pathToKing = GameControls.GetPathToKing(king, attacker);

            if (pathToKing != null &&
                pathToKing.Count > 0 &&
                teamPiece is IChessMoves teamMatePiece &&
                teamMatePiece.AvailableMoves != null &&
                chessBoard.ChessSquares_ != null)
            {
                // Iterate through the available moves of the player's chess piece to check if any can block the check.
                foreach (var teamMate in teamMatePiece.AvailableMoves)
                {
                    // Only consider blocking moves that lie on the attack path and where we have an explicit move owner
                    if (pathToKing.Any(p => p == teamMate.Move && p != king.CurrentLocation) &&
                        teamMate.MoveOwner != null)
                    {
                        // create a deep copy of the board to simulate the blocking move
                        var boardCopy = chessBoard.DeepClone();

                        if (boardCopy.ChessSquares_ == null)
                            continue;

                        // Find the corresponding piece on the copied board using the moveOwner's location
                        var originalSquareInCopy = teamMate.MoveOwner.CurrentLocation.GetSquare(boardCopy.ChessSquares_);
                        var chessPieceCopy = originalSquareInCopy?.ChessPiece_;

                        if (chessPieceCopy == null)
                            continue;

                        // remove the piece from its original square in the copy
                        if (originalSquareInCopy != null)
                            originalSquareInCopy.ChessPiece_ = null;

                        // place the copied piece on the intended blocking square
                        var targetSquareInCopy = teamMate.Move.GetSquare(boardCopy.ChessSquares_);
                        if (targetSquareInCopy == null)
                            continue;

                        MovementControls.PlacePiece(chessPieceCopy, targetSquareInCopy, boardCopy);

                        // find king on the copied board so IsCheck inspects the simulated position correctly
                        var kingSquareInCopy = king.CurrentLocation.GetSquare(boardCopy.ChessSquares_);
                        var kingCopy = kingSquareInCopy?.ChessPiece_ as King;

                        // if after the simulated move the king is no longer in check, the attack is blockable
                        var results = IsCheck(king.PieceColor, kingCopy ?? king, boardCopy);

                        // if the king is not in check after the simulated move, return true indicating the check can be blocked
                        if (!results.IsCheck)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region -- Method --
        /// <summary>
        /// Places a chess piece on the specified location of a copy of the chess board. This method is used for simulating moves without affecting the original board state.
        /// </summary>
        /// <param name="chessPiece">The chess piece to be placed on the board.</param>
        /// <param name="location">The location on the board where the chess piece should be placed.</param>
        /// <param name="copyBoard">The copy of the chess board on which the chess piece will be placed.</param>
        public static void PlaceChessPieceOnBoard(ChessPiece chessPiece, ChessSquareLocation location, Board copyBoard)
        {
            if (copyBoard.ChessSquares_ != null)
            {
                foreach (var square in copyBoard.ChessSquares_)
                {
                    if (square.BoardLocation.Equals(location) && square.ChessPiece_ == null)
                    {
                        square.ChessPiece_ = chessPiece;
                    }
                }
            }
        }

        #endregion

        #region - Reset Method --
        /// <summary>
        /// Resets the check state by clearing the reference to the king and setting the IsPlayerCheck flag to false. 
        /// This method is used to reset the check status after a move has been made or when starting a new game.
        /// </summary>
        public void Reset()
        {
            IsPlayerCheck = false;
            King = null;
        }

        #endregion
    }
}
