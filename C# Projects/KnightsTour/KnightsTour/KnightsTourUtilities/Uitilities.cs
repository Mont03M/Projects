using KnightsTour.Board;
using KnightsTour.Heuristics;
using KnightsTour.Structs;

namespace KnightsTour.KnightsTourUtilities
{
    // Uitilities Manager to handle calling solverUititles and WarndorffsHeuristics classes
    public class Uitilities
    {
        public SolverUitlities solverUitlities { get; set; } = default!;
        public WarnsdorffHeuristics warnsdorffHeuristics { get; set; } = default!;
        public Dictionary<int, List<WarnsdorffRule>> visitedMoves { get; set; } = default!;
        public BoardLocation startLocation { get; set; } = default!;
        public List<WarnsdorffRule> startLocationMoves { get; set; } = default!;
        public BoardLocation[] offsets { get; set; } = default!;
        public ChessBoard ChessBoard { get; set; } = default!;
        public int size { get; set; }

        public Uitilities(int size, Dictionary<int, List<WarnsdorffRule>> visitedMoves,
            BoardLocation startLocation, BoardLocation[] offsets, ChessBoard ChessBoard)
        {
            this.size = size;
            this.visitedMoves = visitedMoves;
            this.startLocation = startLocation;
            this.offsets = offsets;
            this.ChessBoard = ChessBoard;

            // solver and warnsdorffHeuristics objects
            solverUitlities = new SolverUitlities(size, visitedMoves, startLocation, offsets, ChessBoard);
            warnsdorffHeuristics = new WarnsdorffHeuristics(size,startLocation,offsets,ChessBoard, solverUitlities);
        }

        // Set Start Moves
        public void SetStartMovesLocation(List<WarnsdorffRule> nextMoves)
        {
            startLocationMoves = new List<WarnsdorffRule>();
            startLocationMoves = nextMoves;
        }

        // Reset visited moves
        public void ResetVisitedMoves()
        {
            visitedMoves = new Dictionary<int, List<WarnsdorffRule>>();
        }

        public void PlaceKnightOnBoard(BoardLocation move, int currentStep)
        {
            // place knight on the board
            ChessBoard.PlaceKnightOnBoard(move, currentStep);
        }

        public void PrintChessBoard(int currentStep, int PauseTime)
        {
            // print chess board
            ChessBoard.PrintChessBoard(currentStep, PauseTime);
        }

        public void RemoveKnightOnBoard(WarnsdorffRule move)
        {
            // backtrack - Remove knight from the board
            ChessBoard.RemoveKnightOnBoard(move.boardLocation);
        }
    }
}
