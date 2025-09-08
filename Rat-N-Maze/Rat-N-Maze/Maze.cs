using System;

namespace Rat_N_Maze
{
    public struct MazeLoc
    {
        public int x { get; set; }
        public int y { get; set; }

        public MazeLoc(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Maze
    {
        // char array
        public char[,] board = null;
       
        public int size { get; set; }

        // constructor
        public Maze(int n)
        {
            this.size = n;
            board = new char[n, n];

            // initalizes the board
            InitalizeBoard();
            
            CreateMaze();
        }

        // function intializes the board with '.' chars
        public void InitalizeBoard()
        {

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    board[i, j] = ' ';
                }
            }
        }

        // move rat to new location in the maze
        public void PlaceRat(MazeLoc loc)
        {
            CreateMaze();
            board[loc.x, loc.y] = '@';

        }


        // function adds the queens to the board
        public void CreateMaze()
        {
            MazeLoc[] maze =
            {
                new MazeLoc(0, 0),
                new MazeLoc(1, 0),
                new MazeLoc(2, 0),
                new MazeLoc(3, 0),
                new MazeLoc(4, 0),
                new MazeLoc(4, 1),
                new MazeLoc(4, 2),
                new MazeLoc(4, 3),
                new MazeLoc(3, 3),
                new MazeLoc(2, 3),
                new MazeLoc(2, 2),
                new MazeLoc(1, 2),
                new MazeLoc(0, 2),
                new MazeLoc(2, 4),
                new MazeLoc(2, 5),
                new MazeLoc(1, 5),
                new MazeLoc(0, 5),
                new MazeLoc(0, 6),
                new MazeLoc(0, 7),
                new MazeLoc(4, 4),
                new MazeLoc(4, 5),
                new MazeLoc(5, 5),
                new MazeLoc(6, 5),
                new MazeLoc(6, 6),
                new MazeLoc(4, 6),
                new MazeLoc(4, 7),
                new MazeLoc(6, 7),
                new MazeLoc(7, 7),
                new MazeLoc(5, 3),
                new MazeLoc(6, 3),
                new MazeLoc(7, 3),
                new MazeLoc(7, 4),
                new MazeLoc(6, 2),
                new MazeLoc(6, 1),
                new MazeLoc(7, 1),


            };

           // create maze
           foreach(MazeLoc m in maze)
           {
                 board[m.x, m.y] = '.';
           } 
        }

        // function prints the board
        public void PrintMaze()
        {

            // header 
            Console.ForegroundColor = ConsoleColor.White;

            // printing the line of each column
            // header lines
            for (int l = 0; l < size; l++)
            {
                if( l == 0)
                {
                    Console.Write("#");
                }

                Console.Write("---#");
            }
           
            Console.Write("\n");

            // printing the board
            for (int i = 0; i < size; i++)
            {

                

                for (int j = 0; j < size; j++)
                {
                    // calling function to print the stored chars and within the board
                    PlaceDirection(board[i, j]);
                }

                if (i == size-1)
                    Console.Write("--->");

                // printing the bottom of the grid
                Console.Write("|\r\n");

                for (int k = 0; k < size; k++)
                {

                    if(k == 0)
                    {
                        Console.Write("#");
                    }

                    Console.Write("---#");

                }
               
                Console.Write("\n\r");
            }
        }
        // function determines the appropriate char to place based on the stored char within the array board
        public void PlaceDirection(char place)
        {
            switch (place)
            {
                case ' ':
                    Console.Write("|");
                    Console.BackgroundColor = ConsoleColor.White;
                  
                    Console.Write(" - ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.ResetColor();
                    break;

                case '.':
                    Console.Write("|");
                    // Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" . ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case '@':
                    Console.Write("|");
                    // Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" @ ");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;

            }
        }
    }
}
