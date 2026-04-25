using KnightsTour.Solver;
using KnightsTour.Board;
using KnightsTour.Structs;
namespace knightsTourSolverAlgo;

public class KnightsTour
{
    public static void Main(string[] args)
    {
        bool startGame = true;

        bool isClosedTour = false;

        List<BoardLocation> chessBoardLocations;

        // while the user continues to prompt to solve knight tours
        while (startGame)
        {
            Console.Clear();

            // take user input
            var size = SizeInput("Enter a board size to start Knight Tour Solver (e.g, 5-20): ", 5, 20);

            Console.WriteLine("\n");

            // get starting location
            var startLocationIndex = PromptBoard(new ChessBoard(size), "Enter a starting location: ", 1, 
                (size*size), out chessBoardLocations);

            Console.Write("\n");

            // tour closed or open ?
            isClosedTour = PromptUser("Closed Tour [Y/N] ?: ");
           
            // Solve the Knights Tour problem
            new KnightTourSolver(new ChessBoard(size), isClosedTour, pauseTime: 100)
                    .KnightsTourDriver(chessBoardLocations[startLocationIndex], chessBoardLocations);

            // prompt for new game
            if (!PromptUser("Enter another tour [Y/N] ?: "))
            {
                startGame = false;
            }
        }
        
    }

    // Continues to prompt for user input, unitl valid size input is entered 
    public static int SizeInput(string message, int lowerLimit, int upperLimit)
    {
        int size = 0;

        while (true)
        {
            Console.Write(message);

            string? boardSize = Console.ReadLine();

            if(int.TryParse(boardSize, out size) && size >= lowerLimit && size <= upperLimit)
            {
                return size;
            }
        }
    }

    // continues to prompt the user for correct input (Y,y, N,n)
    public static bool PromptUser(string message)
    {
        while (true)
        {
            Console.Write(message);

            string? promptInput = Console.ReadLine();

            if (promptInput != null && promptInput.ToUpper().Equals("Y"))
                return true;

            else if (promptInput != null && promptInput.ToUpper().Equals("N"))
                return false;
        }
    }

    // Creates repersentation of a chess board depending on the size enter by the user 
    public static int PromptBoard(ChessBoard board, string message, int lowerLimit, int upperLimit, 
        out List<BoardLocation> chessBoardLocations)
    {
        chessBoardLocations = new List<BoardLocation>();

        // create (x,y) coordinates
        for (int x = 0; x < board.Size; x++)
        {
            for (int y = 0; y < board.Size; y++)
            {
                chessBoardLocations.Add(new BoardLocation(x, y));
            }
        }

        // place (x,y) coordinates on the chess board
        foreach(var location in chessBoardLocations)
        {
            board.PlaceKnightOnBoard(location, chessBoardLocations.IndexOf(location)+1);
        }

        board.PrintBoard(false);
        
        // prompt for correct starting location based on the size of the chessboard
        var startLocation = SizeInput(message, lowerLimit, upperLimit);

        // return starting location
        return (startLocation-1);
    }
}