using ChessGame.Enums;

namespace ChessGame.Interfaces.Services
{
    /// <summary>
    /// Represents a service interface for managing the state and behavior of a chess game.
    /// </summary>
    public interface IIChessGameService
    {
        /// <summary>
        /// Gets or sets the selected level of the chess game.
        /// </summary>
        public string? selectedLevel { get; set; }

        /// <summary>
        /// Gets or sets the selected game mode of the chess game.
        /// </summary>
        public string? selectedGameMode { get; set; }

        /// <summary>
        /// Gets or sets the color of the player in the chess game.
        /// </summary>
        public ChessPieceColors playerColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the opponent in the chess game.
        /// </summary>
        public ChessPieceColors opponetColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lighting chess is enabled.
        /// </summary>
        public bool LightingChess { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game is paused.
        /// </summary>
        public bool IsGamePause { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a pawn promotion selection is in progress.
        /// </summary>
        public bool IsPawnPromotionSelection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game is over.
        /// </summary>
        public bool IsGameOver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether white is in checkmate.
        /// </summary>
        public bool IsCheckMateWhite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether black is in checkmate.
        /// </summary>
        public bool IsCheckMateBlack { get; set; }

        /// <summary>
        /// Gets or sets the color of the player whose turn it is.
        /// </summary>
        public ChessPieceColors CurrentTurn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player's turn is completed.
        /// </summary>
        public bool PlayerTurnCompleted { get; set; }

        /// <summary>
        /// Gets or sets an action to reset the player's clock.
        /// </summary>
        public Action? PlayerClockReset { get; set; }

        /// <summary>
        /// Gets or sets an action to reset the game clock.
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
        public Func<string, string, string, string, Task> IsEndOfGame { get; set; }

        /// <summary>
        /// Gets or sets a dictionary to track the occurrences of positions.
        /// </summary>
        public Dictionary<string, int> PositionOccurrences { get; set; }
    }
}
