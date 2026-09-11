using System.Windows.Media;
using ChessGame.ChessPieces;
using ChessGame.Enums;
using ChessGame.Structs;
using ChessGame.Utilities;

namespace ChessGame.Square
{
    /// <summary>
    /// Represents a square on a chessboard, including its color, location, and any chess piece that may occupy it.
    /// </summary>
    public class ChessSquare : ObservableObject
    {
        /// <summary>
        /// The location of the square on the chessboard, represented as a ChessSquareLocation struct.
        /// </summary>
        private ChessPiece? chessPiece;

        /// <summary>
        /// The color of the square, represented as a Brush. This can be used to visually distinguish between different squares on the chessboard.
        /// </summary>
        private Brush color;

        /// <summary>
        /// The location of the square on the chessboard, represented as a ChessSquareLocation struct.
        /// </summary>
        public ChessSquareLocation BoardLocation { get; set; }

        /// <summary>
        /// The location of the square on the chessboard in standard chess notation (e.g., "e4", "d5"). This is a string representation of the square's position.
        /// </summary>
        public string ChessBoardLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the chess piece that occupies this square. If the square is empty, this property will be null.
        /// </summary>
        public ChessPiece? ChessPiece_
        {
            get => chessPiece;

            set
            {
                chessPiece = value;
                OnPropertyChanged(nameof(ChessPiece_));
            }
        }

        /// <summary>
        /// Gets or sets the color of the square. This property is of type Brush, allowing for various color representations. 
        /// When set, it freezes the brush if possible to ensure thread safety when used across different threads.
        /// </summary>
        public Brush Color_
        {
            get => color;
            set
            {
                // Freeze Freezable brushes so they are thread-safe when used across threads
                if (value is System.Windows.Freezable freezable && freezable.CanFreeze)
                {
                    freezable.Freeze();
                }

                color = value;
                OnPropertyChanged(nameof(Color_));
            }
        }

        /// <summary>
        /// Initializes a new instance of the ChessSquare class with the specified color, board location, chess board location, and an optional chess piece.
        /// </summary>
        /// <param name="color">The color of the square.</param>
        /// <param name="boardLocation">The location of the square on the chessboard.</param>
        /// <param name="chessBoardLocation">The location of the square on the chessboard in standard chess notation.</param>
        /// <param name="chessPiece">The chess piece that occupies this square, if any.</param>
        public ChessSquare(Brush color, ChessSquareLocation boardLocation, string chessBoardLocation = "", ChessPiece chessPiece = null!)
        {

            if (color is System.Windows.Freezable freez && freez.CanFreeze)
            {
                freez.Freeze();
            }
            this.color = color;

            this.BoardLocation = boardLocation;
            this.ChessPiece_ = chessPiece;
            this.ChessBoardLocation = chessBoardLocation;

        }

        /// <summary>
        /// Creates a deep copy of the current ChessSquare instance, including its color, location, and any chess piece that may occupy it.
        /// </summary>
        /// <returns>A new ChessSquare instance that is a deep copy of the current instance.</returns>
        public ChessSquare Clone()
        {
            switch (ChessPiece_?.PieceType)
            {
                case PieceType.PAWN:
                    Pawn pawn = (Pawn)ChessPiece_;
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y),
                           this.ChessBoardLocation,
                           pawn.DeepClone_());

                case PieceType.KNIGHT:
                    Knight knight = (Knight)ChessPiece_;
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y),
                           this.ChessBoardLocation,
                           knight.DeepClone_());

                case PieceType.BISHOP:
                    Bishop bishop = (Bishop)ChessPiece_;
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y),
                           this.ChessBoardLocation,
                           bishop.DeepClone_());

                case PieceType.ROOK:
                    Rook rook = (Rook)ChessPiece_;
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y),
                           this.ChessBoardLocation,
                           rook.DeepClone_());

                case PieceType.QUEEN:
                    Queen queen = (Queen)ChessPiece_;
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y),
                           this.ChessBoardLocation,
                           queen.DeepClone_());

                case PieceType.KING:
                    King king = (King)ChessPiece_;
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y),
                           this.ChessBoardLocation,
                           king.DeepClone_());
                default:
                    return new ChessSquare(Color_, new ChessSquareLocation(BoardLocation.X, BoardLocation.Y), ChessBoardLocation);
            }
        }

        /// <summary>
        /// Returns a string representation of the ChessSquare instance, primarily showing its chess board location.
        /// </summary>
        /// <returns>A string representation of the ChessSquare instance.</returns>
        public override string ToString()
        {
            return $"{ChessBoardLocation}  ";
        }
    }
}
