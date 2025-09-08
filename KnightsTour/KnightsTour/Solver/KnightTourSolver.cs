using KnightsTour.Board;
using KnightsTour.Heuristics;
using KnightsTour.KnightsTourUtilities;
using KnightsTour.Structs;

namespace KnightsTour.Solver
{
    public class KnightTourSolver
    {
        private int PauseTime { get; set; }
        private ChessBoard ChessBoard { get; set; }
        private bool IsClosedTour { get; }

        // Knights offsets for moves
        private BoardLocation[] OffSetValues =
        [
            new (2, 1),
            new (1, 2),
            new (-1, 2),
            new (-2, 1),
            new (-2,-1),
            new (-1, -2),
            new (1, -2),
            new (2, -1)
        ];

        private BoardLocation startLocation { get; set; }
        public KnightTourSolver(ChessBoard board, bool isClosedTour, int pauseTime)
        {
            ChessBoard = board;
            IsClosedTour = isClosedTour;
            PauseTime = pauseTime;
        }

        public bool KnightsTour(BoardLocation loc,int step, Uitilities uitilitiesMananager)
        {
            List<WarnsdorffRule> nextMoves = new List<WarnsdorffRule>();

            var currentStep = step + 1;

            // solution found ? 
            if (uitilitiesMananager.solverUitlities.IsTourComplete(step, IsClosedTour))
            {
                BoardLocation location = loc;

                return uitilitiesMananager.solverUitlities.HandleIsTourComplete(currentStep, 
                    IsClosedTour, PauseTime, location, out location);
            }
           
            // generate moves for the current step
            nextMoves = uitilitiesMananager.warnsdorffHeuristics.WarnsdorffHeuristic(nextMoves, loc,currentStep);

            // start the moves the starting location
            if (step == 1)
                uitilitiesMananager.SetStartMovesLocation(nextMoves);

            foreach (var move in nextMoves.ToList())
            {
                // if closed tour -- back track early if no move is found 
                if (uitilitiesMananager.solverUitlities.BreakEarly(
                    IsClosedTour, currentStep, uitilitiesMananager.startLocationMoves)) 
                    break;

                // add move to visited list
                uitilitiesMananager.solverUitlities.AddMoveToList(currentStep, move);

                // place knight on the board
                uitilitiesMananager.PlaceKnightOnBoard(move.boardLocation, currentStep);
               
                uitilitiesMananager.PrintChessBoard(currentStep, PauseTime);

                // recursive call -- move on to the next location 
                if (KnightsTour(move.boardLocation,currentStep, uitilitiesMananager))
                    return true;

                uitilitiesMananager.PrintChessBoard(currentStep, PauseTime);

                // backtrack - Remove knight from the board
                uitilitiesMananager.RemoveKnightOnBoard(move);
               
                // backtrack - reset visited moves
                if (currentStep - 1 == 1)
                {
                    uitilitiesMananager.ResetVisitedMoves();
                }
            }
           
            // solution not found
            return false;
        }
        

        // Driver Function
        public void KnightsTourDriver(BoardLocation startLocation, List<BoardLocation> chessBoardLocations)
        {
            var uitilitiesMananager = new Uitilities(ChessBoard.Size,
                new Dictionary<int, List<WarnsdorffRule>>(),
                startLocation, OffSetValues, ChessBoard);

            // Shuffling knight offset values
            uitilitiesMananager.solverUitlities.ShuffleOffsetValues();

            // place the start index on the board
            uitilitiesMananager.PlaceKnightOnBoard(startLocation, 1);

            // print the board
            uitilitiesMananager.PrintChessBoard(1, PauseTime);

            // solve the knights tour problem
            var solved = KnightsTour(startLocation, 1, uitilitiesMananager);

            PrintResults(solved, startLocation, chessBoardLocations);
        }

        public void PrintResults(bool solved, BoardLocation IsSloved, List<BoardLocation> chessBoardLocations)
        {
            // print results
            if (solved)
                Console.WriteLine($"Puzzle solved: Board Size - {ChessBoard.Size} * {ChessBoard.Size} = " +
                    $"{ChessBoard.Size * ChessBoard.Size}");
            else
                Console.WriteLine($"Solution Not Found: Board Size - {ChessBoard.Size} * {ChessBoard.Size} = " +
                    $"{ChessBoard.Size * ChessBoard.Size}");

            Console.WriteLine($"Starting Location: {chessBoardLocations.IndexOf(IsSloved) + 1}\n" +
                    $"Starting Coordinates: ({startLocation.x}, {startLocation.y})\n" +
                    $"Closed Tour: {IsClosedTour}\n");
        }
    }
}
