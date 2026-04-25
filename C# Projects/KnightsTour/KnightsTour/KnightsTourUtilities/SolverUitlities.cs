using KnightsTour.Board;
using KnightsTour.DictonaryExtensions;
using KnightsTour.Heuristics;
using KnightsTour.Structs;

namespace KnightsTour.KnightsTourUtilities
{
    public class SolverUitlities
    {
        private Dictionary<int, List<WarnsdorffRule>> visitedMoves { get; set; }

        private BoardLocation startLocation { get; set; }

        private BoardLocation[] offsets { get; set; }

        private ChessBoard ChessBoard { get; set; }

        private int size { get; set; }

        public SolverUitlities(int size, Dictionary<int, List<WarnsdorffRule>> visitedMoves,
            BoardLocation startLocation, BoardLocation[] offsets, ChessBoard ChessBoard)
        {
            this.size = size;
            this.visitedMoves = visitedMoves;
            this.startLocation = startLocation;
            this.offsets = offsets;
            this.ChessBoard = ChessBoard;
        }

        // Method Generates moves for the Knight
        public virtual List<WarnsdorffRule> GenerateMoves(List<WarnsdorffRule> nextMoves, BoardLocation loc, int step)
        {
            // loop through all possible moves
            for (int i = 0; i < 8; i++)
            {
                // generate next move
                BoardLocation next = new BoardLocation(loc.x + offsets[i].x, loc.y + offsets[i].y);

                WarnsdorffRule heuristicOp = new WarnsdorffRule(next, 0);

                // Check if is in bounds and is an empty square -- check if the location has been visited by the current step
                if (next.IsSafe(ChessBoard) && !visitedMoves.Contains_(step, heuristicOp))
                    nextMoves.Add(heuristicOp);

            }

            return nextMoves;
        }

        // Generate Moves specifically for checking if backtracking early is avaiable
        public List<WarnsdorffRule> GenerateBackTrackMoves(BoardLocation startLocation, 
            BoardLocation loc, List<BoardLocation> movesChecked)
        {
            List<WarnsdorffRule> moves = new List<WarnsdorffRule>();

            for (int i = 0; i < 8; i++)
            {
                // generate next move
                BoardLocation next = new BoardLocation(loc.x + offsets[i].x, loc.y + offsets[i].y);

                // Check move is in bounds and does not equal the starting location
                if (next.IsMoveInBounds(ChessBoard) && next != startLocation
                    && !movesChecked.Contains(next))
                    moves.Add(new WarnsdorffRule(next, 0));
                
            }

            return moves;
        }

        // Method check if the current step completes the closed tour
        public bool ClosedTour(BoardLocation loc, out BoardLocation location)
        {
            // check if square before the last sqaure will form a closed tour
            foreach (var move in offsets)
            {
                // generate the the next set of knights moves
                BoardLocation isClosedTour = new BoardLocation(loc.x + move.x, loc.y + move.y);

                // is square safe
                if (isClosedTour.IsSafe(ChessBoard))
                {
                    // checking if last square forms the closed tour
                    foreach (var move_ in offsets)
                    {
                        // generate the next move (matching for starting point)
                        BoardLocation startingPoint = new BoardLocation(
                            isClosedTour.x + move_.x, isClosedTour.y + move_.y);

                        if (startingPoint.IsMoveInBounds(ChessBoard))
                        {
                            // last square equal the starting square ? 
                            if (startingPoint == startLocation)
                            {
                                // return the location of the last square
                                location = isClosedTour;
                                return true;
                            }
                        }

                    }
                }
            }
            // return the location of the square before the last sqaure 
            location = loc;

            return false;
        }

        // Method determines if backtracking early is necessary to solve the tour
        public bool BackTrackEarly(List<WarnsdorffRule> startLocationMoves, int step)
        {
            List<BoardLocation> movesChecked = new List<BoardLocation>();

            int stepCheck = ChessBoard.Size * ChessBoard.Size; // setting the steps - based on board size

            int Threshold = (stepCheck - 5) - 1; // setting the threshold to stop checking if a path is available

            var start = startLocation;
            var currentLocation = startLocation;

            // check path is obtainable from starting location
            while (stepCheck != Threshold)
            {
                // generate moves 
                var moves = GenerateBackTrackMoves(start, currentLocation, movesChecked);


                // if any move of the starting locations moves are not available - backtrack 
                if (currentLocation == startLocation
                    && !startLocationMoves.Any(x => x.boardLocation.EqualsComparedValue(".", ChessBoard)))
                {
                    return true;
                }

                // determine if a path is open for a closed tour
                if ((moves.Any(x => x.boardLocation.EqualsComparedValue(".", ChessBoard))
                    || moves.Any(x => x.boardLocation.EqualsStep(step - 1, ChessBoard))))
                {
                    // determine if moves contains any available moves
                    var validMove = moves.FirstOrDefault(x => x.boardLocation.EqualsComparedValue(".", ChessBoard));

                    // if valid - check the next location
                    if (validMove != null)
                    {
                        currentLocation = validMove.boardLocation;
                        movesChecked.Add(currentLocation);
                    }
                }
                else
                {
                    return true;
                }
                
                stepCheck -= 1;
            }
            
            // path is reachable
            return false;
        }

        
        // Method determines if the tour is complete
        public bool IsTourComplete(int step, bool IsClosedTour)
        {
            return (step == size * size - 1 && IsClosedTour || step == size * size);
        }

        // Method determines if the current solved tour is closed or open tour
        public bool HandleIsTourComplete(int step, bool IsClosedTour, int pauseTime, 
            BoardLocation loc, out BoardLocation location)
        {
            // Closed tour
            if (IsClosedTour)
            {
                // determine if closed tour is solved
                var IsClosed = ClosedTour(loc, out location);

                // place the last knight on the board
                ChessBoard.PlaceKnightOnBoard(location, step);

                // print solution
                ChessBoard.PrintChessBoard(step,pauseTime);

                // isTrue ? true : false
                return IsClosed;

            }

            // if not a closed tour set the location to the starting location
            location = loc;

            return true;
        }

        // Method determine whether to call BackTrackEarly Method
        public bool BreakEarly(bool IsClosedTour, int step, List<WarnsdorffRule> startLocationMoves)
        {
            if (!IsClosedTour)
                return false;
            return (BackTrackEarly(startLocationMoves, step));
        }

        // Adds a move to the visited moves dictonary
        public void AddMoveToList(int currentStep, WarnsdorffRule move)
        {
            if (!visitedMoves.ContainsKey(currentStep))
                visitedMoves[currentStep] = new List<WarnsdorffRule>();
            visitedMoves[currentStep].Add(move);
        }

        // shuffles values in an out-of-order ordering
        public void ShuffleOffsetValues()
        {
            var rnd = new Random();
            rnd.Shuffle(offsets);
        }
    }
}
