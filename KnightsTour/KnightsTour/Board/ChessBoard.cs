using KnightsTour.Structs;

namespace KnightsTour.Board
{
    public class ChessBoard
    {
        // char array
        public string[,] board = null!;

        public int Size { get; set; }

        // constructor
        public ChessBoard(int n)
        {
            Size = n;
            board = new string[n, n];

            // initalize the board
            InitalizeBoard();
        }

        // intializes the board with '.' chars
        public void InitalizeBoard()
        {

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    board[i, j] = ".";
                }
            }
        }

        
        // place knight on the board
        public void PlaceKnightOnBoard(BoardLocation loc, int step)
        {
            board[loc.x, loc.y] = step.ToString();
        }

        // remove knight from the board
        public void RemoveKnightOnBoard(BoardLocation loc)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if(i == loc.x && j == loc.y)
                    {
                        board[loc.x, loc.y] = ".";
                    }
                }
            }
        }

        // print the board
        public void PrintBoard(bool gameBoard = true)
        {
            // header 
            if (gameBoard)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("=========================== Knights Open Tour ===========================\n");
                Console.WriteLine($"================= Chess Board Size: {Size} * {Size} ====================\n\n");
            }
            
            // printing the line of each column
            // header lines
            for (int l = 0; l < Size; l++)
            {
                if (l == 0)
                {
                    Console.Write("#");
                }

                Console.Write("-----#");
            }

            Console.Write("\n");

            // printing the board
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    // calling function to print the stored chars within the board
                    PlaceKnight(board[i, j]);
                }

                // printing the bottom of the grid
                Console.Write("|\r\n");

                for (int k = 0; k < Size; k++)
                {
                    if (k == 0)
                    {
                        Console.Write("#");
                    }

                    Console.Write("-----#");
                }

                Console.Write("\n\r");
            }
        }

        // Determining the appropriate char to place, based on the stored char within the array board
        public void PlaceKnight(string place)
        {
            switch (place)
            {
                case " ":
                    Console.Write("|");
                    Console.BackgroundColor = ConsoleColor.White;

                    Console.Write(" - ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ResetColor();
                    break;

                case ".":
                    Console.Write("|");
                    // Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("  .  ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case string dight when int.TryParse(place, out _):
                    Console.Write("|");
                    // Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Blue;

                    if (int.Parse(dight) <= 9)
                        Console.Write($"  {place}  ");
                    else if (int.Parse(dight) < 100)
                        Console.Write($" {place}  ");
                    else
                        Console.Write($" {place} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }

        // print the chess board
        public void PrintChessBoard(int step, int pauseTime)
        {
            // clear all visible and not visible chars on the console
            Console.Write("\u001b[2J\u001b[H\u001b[3J");

            PrintBoard();
            Console.WriteLine($"Current Step: {step}\n");
            Thread.Sleep(pauseTime);
        }

    }
}



