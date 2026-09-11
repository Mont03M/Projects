using ChessGame.Enums;
using ChessGame.Interfaces.Services;

namespace ChessGame.Services
{
    /// <summary>
    /// Represents a service for managing the state and behavior of a chess game. 
    /// This class implements the IIChessGameService interface and provides properties and methods to track game settings, player turns, timers, and game status.
    /// </summary>
    public class IChessGameService : IIChessGameService
    {
        /// <summary>
        /// Gets or sets the selected level of the chess game. This property can be null if no level is selected.
        /// </summary>
        public string? selectedLevel { get; set; }

        /// <summary>
        /// Gets or sets the selected game mode of the chess game. This property can be null if no game mode is selected.
        /// </summary>
        public string? selectedGameMode { get; set; }

        /// <summary>
        /// Gets or sets the color of the player's chess pieces.
        /// </summary>
        public ChessPieceColors playerColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the opponent's chess pieces.
        /// </summary>
        public ChessPieceColors opponetColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the chessboard should be displayed with lighting effects.
        /// </summary>
        public bool LightingChess { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the game is paused.
        /// </summary>
        public bool IsGamePause { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether a pawn promotion selection is in progress.
        /// </summary>
        public bool IsPawnPromotionSelection { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the game is over.
        /// </summary>
        public bool IsGameOver { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the white player is in checkmate.
        /// </summary>
        public bool IsCheckMateWhite { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the black player is in checkmate.
        /// </summary>
        public bool IsCheckMateBlack { get; set; } = false;

        /// <summary>
        /// Gets or sets the color of the player whose turn it is currently.
        /// </summary>
        public ChessPieceColors CurrentTurn { get; set; } = ChessPieceColors.WHITE;

        /// <summary>
        /// Gets or sets a value indicating whether the player's turn has been completed.
        /// </summary>
        public bool PlayerTurnCompleted { get; set; } = false;

        /// <summary>
        /// Gets or sets an action to reset the player's clock.
        /// </summary>
        public Action? PlayerClockReset { get; set; }

        /// <summary>
        /// Gets or sets an action to reset the game's clock.
        /// </summary>
        public Action? GameClockReset { get; set; }

        /// <summary>
        /// Gets or sets an action to stop the player's timer.
        /// </summary>
        public Action? StopPlayerTimer { get; set; }

        /// <summary>
        /// Gets or sets the remaining time for the player.
        /// </summary>
        public TimeSpan PlayerTimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets the remaining time for the game.
        /// </summary>
        public TimeSpan GameTimeRemaining { get; set; }

        /// <summary>
        /// Gets or sets a function to determine if the game has ended.
        /// </summary>
        public required Func<string, string, string, string, Task> IsEndOfGame { get; set; }

        /// <summary>
        /// Gets or sets a dictionary to track the occurrences of positions.
        /// </summary>
        public Dictionary<string, int> PositionOccurrences { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the IChessGameService class.
        /// </summary>
        public IChessGameService() { }
    }
}
