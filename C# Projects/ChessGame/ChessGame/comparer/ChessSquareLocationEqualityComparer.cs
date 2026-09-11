using ChessGame.Structs;

namespace ChessGame.Comparer
{
    /// <summary>
    /// Provides a custom equality comparer for ChessSquareLocation objects, allowing for comparison based on their X and Y coordinates.
    /// </summary>
    public class ChessSquareLocationEqualityComparer : IEqualityComparer<ChessSquareLocation>
    {
        /// <summary>
        /// Determines whether two ChessSquareLocation instances are equal based on their X and Y coordinates.
        /// </summary>
        /// <param name="a">The first ChessSquareLocation to compare.</param>
        /// <param name="b">The second ChessSquareLocation to compare.</param>
        /// <returns>True if the X and Y coordinates of both instances are equal; otherwise, false.</returns>
        public bool Equals(ChessSquareLocation a, ChessSquareLocation b)
        {
            // assume non-nullable parameters per interface contract
            return a.Equals(b.X, b.Y);
        }

        /// <summary>
        /// Returns a hash code for the specified ChessSquareLocation instance, based on its X and Y coordinates.
        /// </summary>
        /// <param name="obj">The ChessSquareLocation instance for which to get the hash code.</param>
        /// <returns>A hash code for the specified ChessSquareLocation instance.</returns>
        public int GetHashCode(ChessSquareLocation obj)
        {
            return (obj.X * 31) ^ obj.Y;
        }
    }
}
