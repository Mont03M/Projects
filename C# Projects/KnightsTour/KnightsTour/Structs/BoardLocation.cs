using KnightsTour.Board;

namespace KnightsTour.Structs
{
    // (x,y) for each square on the chess board
    public struct BoardLocation
    {
        public int x { get; set; }
        public int y { get; set; }
        public double x_ { get; set; }
        public double y_ { get; set; }

        public BoardLocation(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public BoardLocation(double x, double y)
        {
            x_ = x;
            y_ = y;
        }
        
        // Determines if two objects equals
        public override bool Equals(object? obj)
        {
            if (obj is BoardLocation other)
            {
                return x == other.x && y == other.y;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        // operator to determine if two objects of the same type equal
        public static bool operator ==(BoardLocation a, BoardLocation b)
        {
            return a.Equals(b);
        }

        // operator to determine if two objects of the same type are not equal
        public static bool operator !=(BoardLocation a, BoardLocation b)
        {
            return !(a == b);
        }

        // operator to determine if two objects of the same type are greater than or equal
        public static bool operator >=(BoardLocation a, BoardLocation b)
        {
            return (a.x >= b.x) && (a.y >= b.y);
        }

        // operator to determine if two objects of the same type are less than or equal
        public static bool operator <=(BoardLocation a, BoardLocation b)
        {
            return (a.x <= b.x) && (a.y <= b.y);
        }

        // operator to subject two objects of the same type
        public static BoardLocation operator -(BoardLocation a, BoardLocation b)
        {
            return new BoardLocation(a.x - b.x, a.y - b.y);
        }

        // Checks if an (x,y) coordinate is in bounds and on an empty square
        public bool IsSafe(ChessBoard board)
        {
            if(x >= 0 
                && y >= 0
                && x < board.Size
                && y < board.Size 
                && board.board[x, y] == ".")
              return true;
           
            return false;
        }

        // Checks if an (x,y) coordinate is in bounds 
        public bool IsMoveInBounds(ChessBoard board)
        {
            if (x >= 0
                && y >= 0
                && x < board.Size
                && y < board.Size)
                return true;
            return false;

        }

        // Checks if an (x,y) coordinate equals a place marker
        public bool EqualsComparedValue(string placeMarker, ChessBoard ChessBoard)
        {
            if (ChessBoard.board[x, y] == placeMarker)
                return true;
            return false;
        }

        // Checks if an (x,y) coordinate's square equals the current step
        public bool EqualsStep(int step, ChessBoard ChessBoard)
        {
            var location = ChessBoard.board[x, y];

            if (!int.TryParse(location, out _))
                return false;

            if (int.Parse(location) == step)
                return true;

            return false;
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }
    }
}
