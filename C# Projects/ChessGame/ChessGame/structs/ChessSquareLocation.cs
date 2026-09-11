using ChessGame.Square;
using System.Collections.ObjectModel;

namespace ChessGame.Structs
{
    /// <summary>
    /// Represents a location on a chessboard with X and Y coordinates. Provides methods for equality checks, bounds checking, 
    /// and retrieving the corresponding chess square from a collection.
    /// </summary>
    /// <param name="x">The X coordinate of the chess square location.</param>
    /// <param name="y">The Y coordinate of the chess square location.</param>
    public struct ChessSquareLocation(int x, int y)
    {
        /// <summary>
        /// Gets or sets the Y coordinate of the chess square location.
        /// </summary>
        public int Y { get; set; } = y;
        /// <summary>
        /// Gets or sets the X coordinate of the chess square location.
        /// </summary>
        public int X { get; set; } = x;

        /// <summary>
        /// Determines whether the specified object is equal to the current ChessSquareLocation instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current ChessSquareLocation instance.</param>
        /// <returns>true if the specified object is equal to the current ChessSquareLocation instance; otherwise, false.</returns>
        public override readonly bool Equals(object? obj)
        {
            if(obj is ChessSquareLocation other)
            {
                return this.X == other.X && this.Y == other.Y;
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for the current ChessSquareLocation instance, based on its X and Y coordinates.
        /// </summary>
        /// <returns>A hash code for the current ChessSquareLocation instance.</returns>
        public override readonly int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// Determines whether the current ChessSquareLocation instance is equal to the specified X and Y coordinates.
        /// </summary>
        /// <param name="x">The X coordinate to compare.</param>
        /// <param name="y">The Y coordinate to compare.</param>
        /// <returns>true if the current ChessSquareLocation instance is equal to the specified X and Y coordinates; otherwise, false.</returns>
        public readonly bool Equals(int x, int y)
        {
            return this.X == x && this.Y == y;
        }


        /// <summary>
        /// Determines whether the current ChessSquareLocation instance is within the bounds of a standard 8x8 chessboard.
        /// </summary>
        /// <returns>true if the current ChessSquareLocation instance is within the bounds of a standard 8x8 chessboard; otherwise, false.</returns>
        public readonly bool IsInBounds()
        {
            if(this.X >= 0 && this.X < 8 && this.Y >= 0 && this.Y < 8)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the current ChessSquareLocation instance represents a valid move based on the provided collection of chess pieces.
        /// </summary>
        /// <param name="chessPieces">The collection of chess pieces to check against.</param>
        /// <returns>true if the current ChessSquareLocation instance represents a valid move; otherwise, false.</returns>
        // checks if this object contains the same x and y as another location
        public readonly bool IsMove(ObservableCollection<ChessSquare> chessPieces)
        {
            foreach(var piece in chessPieces)
            {
                // chess piece not null
                if (piece.ChessPiece_ != null) {
                    // same x and y locations ??
                    if (this == piece.ChessPiece_.CurrentLocation)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Retrieves the corresponding ChessSquare from the provided collection of chess squares based on the current ChessSquareLocation instance.
        /// </summary>
        /// <param name="chessSquares">The collection of chess squares to search.</param>
        /// <returns>The corresponding ChessSquare if found; otherwise, null.</returns>
        public readonly ChessSquare? GetSquare(ObservableCollection<ChessSquare> chessSquares)
        {
            foreach(var square in chessSquares)
            {
                if(square != null)
                {
                    if (square.BoardLocation == this) 
                    {
                        return square;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether two ChessSquareLocation instances are equal based on their X and Y coordinates.
        /// </summary>
        /// <param name="left">The first ChessSquareLocation instance to compare.</param>
        /// <param name="right">The second ChessSquareLocation instance to compare.</param>
        /// <returns>true if the two ChessSquareLocation instances are equal; otherwise, false.</returns>
        public static bool operator ==(ChessSquareLocation left, ChessSquareLocation right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two ChessSquareLocation instances are not equal based on their X and Y coordinates.
        /// </summary>
        /// <param name="left">The first ChessSquareLocation instance to compare.</param>
        /// <param name="right">The second ChessSquareLocation instance to compare.</param>
        /// <returns>true if the two ChessSquareLocation instances are not equal; otherwise, false.</returns>
        public static bool operator !=(ChessSquareLocation left, ChessSquareLocation right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a string representation of the current ChessSquareLocation instance in the format "(X, Y)".
        /// </summary>
        /// <returns>A string representation of the current ChessSquareLocation instance.</returns>
        public override readonly string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
