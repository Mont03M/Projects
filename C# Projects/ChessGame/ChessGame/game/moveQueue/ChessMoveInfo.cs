using ChessGame.Enums;

namespace ChessGame.Game.MoveQueue
{
    /// <summary>
    /// Represents information about a chess move, including the player's color, the type of chess piece, the move itself, the type of move, and any promotion information if applicable.
    /// </summary>
    /// <param name="playerColor">The color of the player making the move.</param>
    /// <param name="playerChessPiece">The type of chess piece being moved.</param>
    /// <param name="move">The move being made, represented as a string.</param>
    /// <param name="type">The type of move being made.</param>
    /// <param name="promotionChessType">The type of chess piece the pawn is being promoted to, if applicable.</param>
    public class ChessMoveInfo(ChessPieceColors playerColor, PieceType playerChessPiece, string move, MoveType type, PieceType? promotionChessType = null)
    {
        public ChessPieceColors PlayerColor { get; set; } = playerColor;
        public PieceType ChessPieceType { get; set; } = playerChessPiece;
        public string Move { get; set; } = move;
        public MoveType Type { get; set; } = type;
        public PieceType? PromotionChessType { get; set; } = promotionChessType;

        /// <summary>
        /// Returns a string representation of the ChessMoveInfo object, including the player's color, the type of chess piece, the move, and any promotion information if applicable.
        /// </summary>
        /// <returns>A string representation of the ChessMoveInfo object.</returns>
        public override string ToString()
        {
            if(PromotionChessType != null)
                return $"{Capitalize(PlayerColor.ToString())} -- " +
                    $"{Capitalize(ChessPieceType.ToString())} -- {Move} -- Promotion";
            else
                return $"{Capitalize(PlayerColor.ToString())} --" +
                    $"{Capitalize(ChessPieceType.ToString())} -- {Move} -- {Capitalize(Type.ToString())}";
        }

        /// <summary>
        /// Capitalizes the first letter of the input string and converts the rest of the string to lowercase.
        /// </summary>
        /// <param name="value">The string to capitalize.</param>
        /// <returns>The capitalized string.</returns>
        public static string? Capitalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return char.ToUpper(value[0]) + value[1..].ToLower();
        }

        /// <summary>
        /// Gets the character representation of a chess piece based on its type and the player's color.
        /// </summary>
        /// <param name="chessPieceType">The type of the chess piece.</param>
        /// <param name="playerColor">The color of the player.</param>
        /// <param name="argumentOutOfRangeException">The exception to throw if the chess piece type is unknown.</param>
        /// <returns>The character representation of the chess piece.</returns>
        private static char GetPieceCharacter(
            PieceType chessPieceType, ChessPieceColors playerColor, Exception argumentOutOfRangeException)
        {
            char piece = chessPieceType switch
            {
                PieceType.KING => 'k',
                PieceType.QUEEN => 'q',
                PieceType.ROOK => 'r',
                PieceType.BISHOP => 'b',
                PieceType.KNIGHT => 'n',
                PieceType.PAWN => 'p',

                _ => throw argumentOutOfRangeException
            };

            return IsWhite(playerColor)
                ? char.ToUpperInvariant(piece)
                : piece;
        }

        /// <summary>
        /// Determines if the given player color is white.
        /// </summary>
        /// <param name="playerColor">The color of the player.</param>
        /// <returns>True if the player color is white; otherwise, false.</returns>
        private static bool IsWhite(ChessPieceColors playerColor)
        {
            return playerColor == ChessPieceColors.WHITE;
        }

        /// <summary>
        /// Gets an ArgumentOutOfRangeException for an unknown chess piece type.
        /// </summary>
        /// <param name="chessPieceType">The type of the chess piece.</param>
        /// <returns>An ArgumentOutOfRangeException for the unknown chess piece type.</returns>
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(PieceType chessPieceType)
        {
            return new ArgumentOutOfRangeException(
                            nameof(chessPieceType),
                            "Unknown chess piece type.");
        }
    }
}
