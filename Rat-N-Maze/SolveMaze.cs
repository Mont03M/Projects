using System;
using System.Linq;

namespace Rat_N_Maze
{
    // solve maze
    public class SolveMaze
    {
        private Rat rat { get; set; }
        private Maze maze { get; set; }
        private Direction dir { get; set; }
        private MazeLoc loc { get; set; }
        private Maze prevloc { get; set; }
        private bool endOfMaze { get; set; }

        // constructor
        public SolveMaze(Rat r) 
        {
            this.rat = r;
            this.maze = r.maze;

            dir = Direction.FORWARD;

            loc = new MazeLoc(0,0);


        }

        // print results
        public void PrintRatResults((Direction dir, MazeLoc loc) selectedPath)
        {
            Console.Clear();
            maze.PlaceRat(selectedPath.loc);
            maze.PrintMaze();

            Console.WriteLine($"Direction: {selectedPath.Item1.ToString()}" +
                $"  (X: {selectedPath.Item2.x.ToString()}, Y: {selectedPath.Item2.y.ToString()})");
        }

        // pause console 
        public void PauseMaze()
        {
            System.Threading.Thread.Sleep(3000);
        }

        // place rat and show results
        public void Start()
        {
            maze.PlaceRat(loc);
            maze.PrintMaze();
        }

        // determine the next move
        public void NextMove((Direction dir, MazeLoc loc) nextMove)
        {
           // loop while there is a path to follow
            while(nextMove.dir != Direction.NOPATH)
            {
                // next move the end of maze
                if (nextMove.dir == Direction.END)
                {
                    endOfMaze = true;
                    return;
                }

                // pause console
                PauseMaze();

                // find path
                var selectedPath = rat.findPath(nextMove.dir, nextMove.loc);

                // assign path and loc 
                var dir = selectedPath.Item1;
                var loc = selectedPath.Item2;

                // previous selected path a split (multiple path choosen)
                if(rat.maze_navigate.ElementAt(0).direction == Direction.SPLIT)
                {
                    // print results
                    PrintRatResults((dir, loc));

                    // get the top element of stack
                    // split direction
                    var split = rat.maze_navigate.ElementAt(0);

                    // recursive call
                    NextMove((dir, loc));

                    // end of maze ??
                    if (endOfMaze)
                    {
                        break;
                    }

                    // on call back
                    // determine next move based on location of the previous split location
                    dir = split.Item1;
                    loc = split.Item2;

                }

                // assign the to next move to determine path to follow
                nextMove.loc = loc;
                nextMove.dir = dir;

                // print results
                PrintRatResults((dir, loc));
            }
           
        }
        
        // driver function
        public void NavigateMaze()
        {
            // end of maze has not been reached
            endOfMaze = false;
            // rat find path to the end of the maze
            NextMove((Direction.FORWARD, new MazeLoc(0,0)));
            Console.WriteLine("Mazed Solved");
        }
    }
}

