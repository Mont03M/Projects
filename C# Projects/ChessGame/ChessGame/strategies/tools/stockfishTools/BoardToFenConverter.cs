using ChessGame.ChessBoard;
using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Square;
using System.Text;

namespace ChessGame.Strategies.Tools.StockfishTools
{
    /// <summary>
    /// Provides functionality to convert a chess board state into Forsyth-Edwards Notation (FEN) string representation.
    /// </summary>
    public static class BoardToFenConverter
    {
        /// <summary>
        /// Converts the given chess board state into a FEN string representation.
        /// </summary>
        /// <param name="board">The chess board to convert.</param>
        /// <param name="sideToMove">The side to move next.</param>
        /// <param name="whiteOnBottomScreen">Indicates if white pieces are on the bottom of the screen.</param>
        /// <param name="whiteCanCastleKingSide">Indicates if white can castle king-side.</param>
        /// <param name="whiteCanCastleQueenSide">Indicates if white can castle queen-side.</param>
        /// <param name="blackCanCastleKingSide">Indicates if black can castle king-side.</param>
        /// <param name="blackCanCastleQueenSide">Indicates if black can castle queen-side.</param>
        /// <param name="enPassantTarget">The target square for en passant capture.</param>
        /// <param name="halfmoveClock">The number of halfmoves since the last capture or pawn move.</param>
        /// <param name="fullmoveNumber">The number of the full move.</param>
        /// <returns>The FEN string representation of the board.</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ToFen(
            Board board,
            ChessPieceColors sideToMove,
            bool whiteOnBottomScreen = true,
            bool whiteCanCastleKingSide = true,
            bool whiteCanCastleQueenSide = true,
            bool blackCanCastleKingSide = true,
            bool blackCanCastleQueenSide = true,
            string enPassantTarget = "-",
            int halfmoveClock = 0,
            int fullmoveNumber = 1)
        {
            ArgumentNullException.ThrowIfNull(board);

            if (board.ChessSquares_ is null)
                throw new ArgumentException(
                    "The board does not contain any chess squares.",
                    nameof(board));

            StringBuilder fen = new();

            // ---------------------------------------------------------
            // 1. Piece placement
            // ---------------------------------------------------------

            int fileStart = whiteOnBottomScreen ? 0 : 7;
            int fileEnd = whiteOnBottomScreen ? 8 : -1;
            int fileStep = whiteOnBottomScreen ? 1 : -1;

            int rankStart = whiteOnBottomScreen ? 0 : 7;
            int rankEnd = whiteOnBottomScreen ? 8 : -1;
            int rankStep = whiteOnBottomScreen ? 1 : -1;


            for (int file = fileStart; file != fileEnd; file += fileStep)
            {
                int emptySquares = 0;

                for (int rank = rankStart; rank != rankEnd; rank += rankStep)
                {
                    ChessSquare? square = GetSquare(board, file, rank);

                    if (square?.ChessPiece_ is null)
                    {
                        emptySquares++;
                        continue;
                    }

                    if (emptySquares > 0)
                    {
                        fen.Append(emptySquares);
                        emptySquares = 0;
                    }

                    fen.Append(GetPieceCharacter(square.ChessPiece_, GetArgumentOutOfRangeException(square.ChessPiece_)));
                }

                if (emptySquares > 0)
                    fen.Append(emptySquares);

                if (file != fileEnd - fileStep)
                    fen.Append('/');
            }

            // ---------------------------------------------------------
            // 2. Side to move
            // ---------------------------------------------------------

            fen.Append(' ');

            fen.Append(
                sideToMove == ChessPieceColors.WHITE
                    ? 'w'
                    : 'b');

            // ---------------------------------------------------------
            // 3. Castling rights
            // ---------------------------------------------------------

            fen.Append(' ');

            string castlingRights = GetCastlingRights(
                whiteCanCastleKingSide,
                whiteCanCastleQueenSide,
                blackCanCastleKingSide,
                blackCanCastleQueenSide);

            fen.Append(castlingRights);

            // ---------------------------------------------------------
            // 4. En passant target square
            // ---------------------------------------------------------

            fen.Append(' ');
            fen.Append(
                string.IsNullOrWhiteSpace(enPassantTarget)
                    ? "-"
                    : enPassantTarget);

            // ---------------------------------------------------------
            // 5. Halfmove clock
            // ---------------------------------------------------------

            fen.Append(' ');
            fen.Append(halfmoveClock);

            // ---------------------------------------------------------
            // 6. Fullmove number
            // ---------------------------------------------------------

            fen.Append(' ');
            fen.Append(fullmoveNumber);

            return fen.ToString();
        }

        /// <summary>
        /// Retrieves the chess square at the specified file and rank from the given board.
        /// </summary>
        /// <param name="board">The chess board to search.</param>
        /// <param name="file">The file (column) of the square.</param>
        /// <param name="rank">The rank (row) of the square.</param>
        /// <returns>The chess square at the specified location, or null if not found.</returns>
        private static ChessSquare? GetSquare(
            Board board,
            int file,
            int rank)
        {
            return board.ChessSquares_?
                .FirstOrDefault(square =>
                    square.BoardLocation.X == file &&
                    square.BoardLocation.Y == rank);
        }

        /// <summary>
        /// Gets an ArgumentOutOfRangeException for an unknown chess piece type.
        /// </summary>
        /// <param name="chessPiece">The chess piece for which to get the exception.</param>
        /// <returns>An ArgumentOutOfRangeException for the unknown chess piece type.</returns>
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ChessPiece chessPiece)
        {
            return new ArgumentOutOfRangeException(
                            nameof(chessPiece.PieceType),
                            chessPiece.PieceType,
                            "Unknown chess piece type.");
        }

        /// <summary>
        /// Gets the character representation of a chess piece for FEN notation.
        /// </summary>
        /// <param name="chessPiece">The chess piece for which to get the character representation.</param>
        /// <param name="argumentOutOfRangeException">The exception to throw if the chess piece type is unknown.</param>
        /// <returns>The character representation of the chess piece for FEN notation.</returns>
        private static char GetPieceCharacter(
            ChessPiece chessPiece, Exception argumentOutOfRangeException)
        {
            char piece = chessPiece.PieceType switch
            {
                PieceType.KING => 'k',
                PieceType.QUEEN => 'q',
                PieceType.ROOK => 'r',
                PieceType.BISHOP => 'b',
                PieceType.KNIGHT => 'n',
                PieceType.PAWN => 'p',

                _ => throw argumentOutOfRangeException
            };

            return IsWhite(chessPiece)
                ? char.ToUpperInvariant(piece)
                : piece;
        }

        /// <summary>
        /// Determines if the given chess piece is white.
        /// </summary>
        /// <param name="chessPiece">The chess piece to check.</param>
        /// <returns>True if the chess piece is white; otherwise, false.</returns>
        private static bool IsWhite(ChessPiece chessPiece)
        {
            return chessPiece.PieceColor == ChessPieceColors.WHITE;
        }
        
        /// <summary>
        /// Gets the castling rights for the given sides.
        /// </summary>
        /// <param name="whiteKingSide">Whether white can castle kingside.</param>
        /// <param name="whiteQueenSide">Whether white can castle queenside.</param>
        /// <param name="blackKingSide">Whether black can castle kingside.</param>
        /// <param name="blackQueenSide">Whether black can castle queenside.</param>
        /// <returns>A string representing the castling rights in FEN notation.</returns>
        private static string GetCastlingRights(
            bool whiteKingSide,
            bool whiteQueenSide,
            bool blackKingSide,
            bool blackQueenSide)
        {
            StringBuilder rights = new();

            if (whiteKingSide)
                rights.Append('K');

            if (whiteQueenSide)
                rights.Append('Q');

            if (blackKingSide)
                rights.Append('k');

            if (blackQueenSide)
                rights.Append('q');

            return rights.Length == 0
                ? "-"
                : rights.ToString();
        }
    }
}

