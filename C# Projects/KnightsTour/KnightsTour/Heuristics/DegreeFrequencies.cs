
namespace KnightsTour.Heuristics
{
    // Class keeps track of each moves onward moves (degree)
    public class DegreeFrequencies
    {
        public WarnsdorffRule nextMove { get; set; }
        public int index { get; set; }

        public DegreeFrequencies(WarnsdorffRule nextMove, int index)
        {
            this.nextMove = nextMove;
            this.index = index;
        }

        public override string ToString()
        {
            return $"Class Degree Frequencies: {nextMove.Degree} -- {nextMove.euclideanDistance} -- {index}";
        }
    }
}
