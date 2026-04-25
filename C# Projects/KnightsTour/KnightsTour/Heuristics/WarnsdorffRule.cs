using KnightsTour.Structs;

namespace KnightsTour.Heuristics
{
    // Class keeps track of necessary objects to impelement Warndroff Rule 
    // selecting moves based on the lowest onward moves(degree)
    public class WarnsdorffRule
    {
        public BoardLocation boardLocation { get; set; }
        public int Degree { get; set; }

        public double euclideanDistance { get; set; }

        public double cornerDistance { get; set;}

        public double edgeBias { get; set; }

        public double distanceToStart { get; set; }

        public double score { get; set; }

        public WarnsdorffRule() { }

        public WarnsdorffRule(BoardLocation location, int numPaths) 
        {
            boardLocation = location;
            Degree = numPaths;
        }

        public override string ToString()
        {
            return $"{boardLocation.x} -- {boardLocation.y}";
        }

        public string ToStringObj()
        {
            return $"Degree: {Degree} -- Euclidean Distance: {euclideanDistance}\n" +
                $"X: {boardLocation.x}\n" +
                $"Y: {boardLocation.y}\n";
        }
    }
}
