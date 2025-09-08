using KnightsTour.Board;
using KnightsTour.Heuristics;
using KnightsTour.Structs;

namespace KnightsTour.KnightsTourUtilities
{
    public class WarnsdorffHeuristics
    {
        private SolverUitlities solverUitlities { get; set; }

        private BoardLocation startLocation { get; set; }

        private BoardLocation[] offsets { get; set; }

        private ChessBoard ChessBoard { get; set; }

        private int size { get; set; }

        public WarnsdorffHeuristics(int size, BoardLocation startLocation, BoardLocation[] offsets,
            ChessBoard ChessBoard, SolverUitlities solverUitlities)
        {
            this.size = size;
            this.startLocation = startLocation;
            this.offsets = offsets;
            this.ChessBoard = ChessBoard;
            this.solverUitlities = solverUitlities;
        }


        // Calculate the degree (number of onwards moves)
        public int CountOnWardMoves(WarnsdorffRule loc)
        {
             foreach (var offset in offsets)
             {
                 BoardLocation next = new BoardLocation(
                        loc.boardLocation.x + offset.x, loc.boardLocation.y + offset.y
                        );

                 if (next.IsSafe(ChessBoard)) loc.Degree++;
             }
            
             return loc.Degree;
        }

        // calculate euclidean distance
        public double CalculateEuclideanDistance(WarnsdorffRule move, int step)
        {
            BoardLocation centerOfBoard = new BoardLocation(size-1 / 2.0, size-1 / 2.0);
            
            // distance formula square(x1-x2)^2 + (y2-y1))
            move.euclideanDistance = Math.Sqrt(
                    Math.Pow(centerOfBoard.x - move.boardLocation.x, 2)
                    + Math.Pow(centerOfBoard.y - move.boardLocation.y, 2));
        
            // weight to reduce influence on eculidean distance
            //double weight = Math.Max(0.0, 1.0 - step / (double)(size * size));
            //move.euclideanDistance *= weight;
   
            return move.euclideanDistance;
        }

        // Calculate edge bias based on (x,y) coordinates
        public double EdgeBias(WarnsdorffRule loc)
        {
            loc.edgeBias = Math.Min(Math.Min(loc.boardLocation.x, size - 1 - loc.boardLocation.y),
                Math.Min(loc.boardLocation.y, size - 1 - loc.boardLocation.y));

            return loc.edgeBias;
        }

        // Calculate the corner distance
        public double CornerDistance(WarnsdorffRule move)
        {
            BoardLocation loc = move.boardLocation;
            int n = size - 1;

            var distance = new[]
            {
                Math.Sqrt(loc.x * loc.x + loc.y * loc.y),
                Math.Sqrt(loc.x * loc.x + (n - loc.y) * (n-loc.y)),
                Math.Sqrt((n-loc.x) * (n-loc.x) + loc.y * loc.y),
                Math.Sqrt((n-loc.x) * (n-loc.x) * (n-loc.y))
            };

            // return the min value
            move.cornerDistance = distance.Min();
            return move.cornerDistance;
        }

        // determine if the step is near the end of the puzzle
        public bool NearEndOfBoard(int step, out double progress)
        {
            double progress_ = (double)step / (size * size);

            // 3/4 of the way completed
            if (progress_ >= 0.75)
            {
                progress = progress_;
                return true;
            }

            progress = 0;

            return false;
            
        }

        // Calculates the eucildean distance from the starting location
        public double CalculatingEucildeanDistanceToStart(WarnsdorffRule move, int step, double progress)
        {
            
            // distance formula square(x1-x2)^2 + (y2-y1))
            move.distanceToStart = Math.Sqrt(
                    Math.Pow(startLocation.x - move.boardLocation.x, 2)
                    + Math.Pow(startLocation.y - move.boardLocation.y, 2));
 
            // weight to reduce influence on eculidean distance
            //double weight = (progress - 0.75) * 4;
            //move.distanceToStart *= weight;
           
            return move.distanceToStart;
        }

        // Method follows Warndroff Rule
        // Calcuates the number of degrees for each move
        // Calculates edge bias (its location to the edge of the board) for each move - tie-breaker 1
        // Calculates corner distance for each move - tie-breaker 2
        // Calculates its distance from the center of the board using the euclidean distance formula - tie breaker 3
        // sorts the list of moves based on degree, then edge bias, corner distance, eucildean distance, then distance from the start location 
        public List<WarnsdorffRule> WarnsdorffHeuristic(List<WarnsdorffRule> nextMoves, BoardLocation loc, int step)
        {

            bool nearEnd = false;


            var moveList = solverUitlities.GenerateMoves(nextMoves, loc,step).Select(m =>
                {
                    // calculating the degree of each move
                    // calculating the euclidean distance from the center of each move
                    m.Degree = CountOnWardMoves(m);
                    m.edgeBias = EdgeBias(m);
                    m.cornerDistance = CornerDistance(m);
                    m.euclideanDistance = CalculateEuclideanDistance(m, step);

                    double progress = 0;
                    nearEnd = NearEndOfBoard(step, out progress);

                    if (nearEnd)
                    {
                       // moveObj.edgeBias = 0;
                       // moveObj.cornerDistance = 0;
                        m.distanceToStart = CalculatingEucildeanDistanceToStart(m, step, progress);
                    }

                    return m;
                })
                .OrderBy(m => m.Degree) // primary - sort by degree - fewest path avilable first
                .ThenBy(m=>m.edgeBias) // then edge bias - tie-breaker 1 -- sort with values considering the center
                .ThenBy(m => m.cornerDistance) // then corner distance - tie-breaker 2 -- sort with values considering the corner of the board
                .ThenBy(m => m.euclideanDistance) // then euclideanDistance - tie-breaker 3 -- sort with values closet to the center
                //.ThenBy(m=> nearEnd ? m.distanceToStart : 0) // then euclidean distance to start location - gravite towards the starting location
                .ToList();
           
            return moveList;
        }
    }
}
