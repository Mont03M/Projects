using ChessGame.ChessBoard;
using ChessGame.ChessPieces;
using ChessGame.Square;
using ChessGame.Colors_;
using ChessGame.Enums;
using ChessGame.Structs;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ChessGame.Interfaces.Game;

namespace ChessGame.Controls
{
    /// <summary>
    /// The SettingControls class is responsible for managing the visual indicators on a chessboard based on user settings. 
    /// It provides methods to highlight available moves, attack moves, special moves, and checkmate conditions for chess pieces. 
    /// The class interacts with the GameControls and AppSettingService to retrieve user preferences and apply them to the chessboard squares.
    /// </summary>
    public class SettingControls : GameControls
    {
        #region -- Properties --
        private GameControls GameControls { get; set; } = default!;

        #endregion

        #region -- Constructors --
        public SettingControls() { }

        public SettingControls(GameControls gameControls)
        {
            GameControls = gameControls;
        }

        #endregion

        #region -- Enabled Indicators --

        /// <summary>
        /// Enables visual indicators on the chessboard squares based on the selected chess piece and user settings.    
        /// </summary>
        /// <param name="choosenSquare">The selected chess square.</param>
        /// <param name="chessBoard">The chess board.</param>
        public void IsEnableIndicators(ChessSquare choosenSquare, Board chessBoard)
        {
            // Check if the settings for available moves, attack moves, and special moves indicators are enabled
            if (GameControls.AppSettingService.Settings.IsEnableAvailableMovesIndicator && choosenSquare != null)
                AvailableMoveIndicator(chessBoard.ChessSquares_, choosenSquare);
            if (GameControls.AppSettingService.Settings.IsEnableAttackMovesIndicator && choosenSquare != null)
                AttackMoveIndicator(chessBoard.ChessSquares_, choosenSquare);
            if (GameControls.AppSettingService.Settings.IsEnableSpecialMoveIndicator && choosenSquare != null)
                SpecialMoveIndicator(chessBoard.ChessSquares_, choosenSquare);
        }

        #endregion

        #region -- Move Indicator Methods (Available, Attack, Special) --

        /// <summary>
        /// Highlights the available moves for the selected chess piece on the chessboard squares based on user settings.
        /// </summary>
        /// <param name="chessSquares">The collection of chessboard squares.</param>
        /// <param name="choosenSquare">The selected chess square.</param>
        /// <param name="settingsAvailableMovesColor">The color to use for highlighting available moves.</param>
        public void AvailableMoveIndicator(ObservableCollection<ChessSquare>? chessSquares, ChessSquare choosenSquare,
            ColorItem? settingsAvailableMovesColor = null)
        {
            if (GameControls.AppSettingService.Settings.availableMovesIndicator == null)
                return;

            // Use the provided color or fallback to the default setting
            Color availableMovesIndicator = settingsAvailableMovesColor?.Color ?? (Color)GameControls.AppSettingService.Settings.availableMovesIndicator;

            if (choosenSquare?.ChessPiece_ is IChessMoves chessMoves &&
                chessSquares is ObservableCollection<ChessSquare> squares)
            {
                // Highlight the squares corresponding to the available moves of the selected chess piece
                foreach (var square in from piece in chessMoves.AvailableMoves?.ToList()
                                           // where piece.moveType != MoveType.CASTLING
                                       from square in chessSquares
                                       where square.BoardLocation.Equals(new ChessSquareLocation(piece.Move.X, piece.Move.Y))
                                       select square)
                {

                    square.Color_ = new SolidColorBrush(availableMovesIndicator);
                }
            }
        }

        /// <summary>
        /// Highlights the special moves for the selected chess piece on the chessboard squares based on user settings.
        /// </summary>
        /// <param name="chessSquares">The collection of chessboard squares.</param>
        /// <param name="choosenSquare">The selected chess square.</param>
        /// <param name="settingsSpecialMovesColor">The color to use for highlighting special moves.</param>
        public void SpecialMoveIndicator(ObservableCollection<ChessSquare>? chessSquares, ChessSquare choosenSquare,
            ColorItem? settingsSpecialMovesColor = null)
        {
            if (GameControls.AppSettingService.Settings.specialMoveIndicator == null)
                return;

            // Use the provided color or fallback to the default setting
            Color specialMovesIndicator = settingsSpecialMovesColor?.Color ?? (Color)GameControls.AppSettingService.Settings.specialMoveIndicator;

            if (choosenSquare?.ChessPiece_ is IPieceSpecialMove chessPiece &&
                chessSquares is ObservableCollection<ChessSquare> squares)
            {
                // Highlight the squares corresponding to the special moves of the selected chess piece
                foreach (var square in from piece in chessPiece.SpecialMoves?.ToList()
                                       from square in chessSquares
                                       where square.BoardLocation.Equals(new ChessSquareLocation(
                                           piece.Move.X, piece.Move.Y))
                                       select square)
                {

                    square.Color_ = new SolidColorBrush(specialMovesIndicator);
                }
            }
        }

        /// <summary>
        /// Highlights all available moves for the selected chess piece on the chessboard squares.
        /// </summary>
        /// <param name="chessSquares">The collection of chessboard squares.</param>
        /// <param name="choosenSquare">The selected chess square.</param>
        public static void AllMovesIndicator(ObservableCollection<ChessSquare>? chessSquares, ChessSquare choosenSquare)
        {
            if (choosenSquare?.ChessPiece_ is IChessMoves chessMoves &&
               chessSquares is ObservableCollection<ChessSquare> squares)
            {
                // Highlight the squares corresponding to all available moves of the selected chess piece
                foreach (var square in from piece in chessMoves.AllAvailableMoves?.ToList()
                                       from square in chessSquares.ToList()
                                       where square.BoardLocation.Equals(new ChessSquareLocation(
                                           piece.Move.X, piece.Move.Y)
                                       )
                                       select square)
                {
                    square.Color_ = new SolidColorBrush(Colors.CadetBlue);
                }
            }
        }

        /// <summary>
        /// Highlights the attack moves for the selected chess piece on the chessboard squares based on user settings.
        /// </summary>
        /// <param name="chessSquares">The collection of chessboard squares.</param>
        /// <param name="choosenSquare">The selected chess square.</param>
        /// <param name="settingsAttackMovesColor">The color to use for highlighting attack moves.</param>
        public void AttackMoveIndicator(ObservableCollection<ChessSquare>? chessSquares, ChessSquare choosenSquare,
            ColorItem? settingsAttackMovesColor = null)
        {
            if (GameControls.AppSettingService.Settings.attackMovesIndicator == null)
                return;

            // Use the provided color or fallback to the default setting
            Color attackMovesIndicator = settingsAttackMovesColor?.Color ?? (Color)GameControls.AppSettingService.Settings.attackMovesIndicator;


            if (choosenSquare?.ChessPiece_ is IChessMoves chessMoves &&
                chessSquares is ObservableCollection<ChessSquare> squares)
            {
                // Highlight the squares corresponding to the attack moves of the selected chess piece
                foreach (var square in from piece in chessMoves.AttackMoves?.ToList()
                                       where piece.MoveType != MoveType.EnPASSENT
                                       from square in chessSquares.ToList()
                                       where square.BoardLocation.Equals(
                                           new ChessSquareLocation(piece.Move.X, piece.Move.Y)
                                           )
                                       select square)
                {
                    square.Color_ = new SolidColorBrush(attackMovesIndicator);
                }
            }
        }

        #endregion

        #region -- Check Indicator Methods --

        /// <summary>
        /// Checks if the king is in check and highlights the squares accordingly based on user settings.
        /// </summary>
        /// <param name="FindKing">A function to find the king piece.</param>
        /// <param name="chessBoard">The chessboard containing the squares.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task IsCheckIndicator(Func<King?> FindKing, Board chessBoard)
        {
            var chessSquares = chessBoard.ChessSquares_;

            // Find the king piece using the provided function
            var king = FindKing.Invoke();

            if (chessSquares != null)
            {
                // Check if the king is in check by iterating through all squares and their chess pieces
                foreach (var square in chessSquares.ToList())
                {
                    if (square.ChessPiece_ != null &&
                        square.ChessPiece_ is IChessMoves chessPiece &&
                        chessPiece.AvailableMoves != null)
                    {
                        // Check if any of the attack moves of the chess piece can attack the king's current location
                        foreach (var move in chessPiece.AttackMoves?.ToList() ?? [])
                        {
                            if (square.ChessPiece_.PieceColor != king?.PieceColor &&
                                move.Move == king?.CurrentLocation)
                            {
                                // Highlight the king's square to indicate check
                                CheckIndicatorHelper(chessSquares, king);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Highlights the square of the king in checkmate on the chessboard based on user settings.
        /// </summary>
        /// <param name="chessSquares">The collection of chessboard squares.</param>
        /// <param name="chessPiece">The chess piece to check for checkmate.</param>
        public void CheckIndicatorHelper(ObservableCollection<ChessSquare>? chessSquares, ChessPiece chessPiece)
        {
            if (GameControls.AppSettingService.Settings.checkMateIndicator == null)
                return;

            if (chessPiece is King king &&
                chessSquares is ObservableCollection<ChessSquare> squares)
            {
                // Highlight the square corresponding to the king's location in checkmate
                foreach (var square in from square in chessSquares.ToList()
                                       where square.BoardLocation.Equals(new ChessSquareLocation(
                                           king.CurrentLocation.X, king.CurrentLocation.Y)
                                       )
                                       select square)
                {
                    square.Color_ = new SolidColorBrush((Color)GameControls.AppSettingService.Settings.checkMateIndicator);
                }
            }
        }

        #endregion

        #region -- Reset Board Squares --

        /// <summary>
        /// Resets the chess squares to their original colors.
        /// </summary>
        /// <param name="chessSquare">The collection of chessboard squares.</param>
        public void ResetBoardSquares(ObservableCollection<ChessSquare>? chessSquare)
        {
            if (chessSquare is not null)
            {
                ChessSquare[] squares = [.. chessSquare];

                int counter = 0;

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        // Reset the color of each square based on its position (odd/even) and user settings
                        squares[counter].Color_ = new SolidColorBrush(((i + j) % 2 == 0 ?
                            GameControls.AppSettingService.Settings.squareOdd ?? Colors.White : GameControls.AppSettingService.Settings.squareEven ?? Colors.Black));
                        counter++;
                    }
                }
            }
        }

        #endregion
    }
}
