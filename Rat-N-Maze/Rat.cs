using System;
using System.Collections.Generic;
using System.Linq;

namespace Rat_N_Maze
{
    // directions
    public enum Direction
    {
        FORWARD,
        BACKWARD,
        LEFT,
        RIGHT,
        SPLIT,
        NOPATH,
        START, 
        END
    }
    public class Rat
    {
        // maze
        public Maze maze { get; set; }

        // stack of directions and paths taken
        public Stack<(Direction direction, MazeLoc location)> maze_navigate;
        
        private Random rand;

        // size of board
        public int size = 0;

        public Rat(Maze maze)
        {

            this.maze = maze;

            rand = new Random(Guid.NewGuid().GetHashCode());

            maze_navigate = new Stack<(Direction direction, MazeLoc location)>();
        }

        // find an open path
        public (Direction, MazeLoc) findPath(Direction direction, MazeLoc currentLocation)
        {
            // end of maze ??
            if (currentLocation.x == maze.size - 1 && currentLocation.y == maze.size - 1)
            {
                return (Direction.END, currentLocation);
            }

            // push previous rat location if not in stack
            if (!maze_navigate.Contains((direction, currentLocation))) maze_navigate.Push((direction, currentLocation));


            // finding paths
            var paths = PathFinder(currentLocation);

            // select a path at random
            int index = SelectRandomPath(paths);

            // only one path available
            if (paths != null && paths.Count == 1)
            {
                return (paths[index].Item1, paths[index].Item2);
            }
            // more than one path available
            else if (paths != null && paths.Count > 1)
            {
                // pop the previous value on top of stack
                maze_navigate.Pop();

                // pushing new value with changed direction
                maze_navigate.Push((Direction.SPLIT, currentLocation));

                return (paths[index].Item1, paths[index].Item2);
            }

            // no path found
            return (Direction.NOPATH, currentLocation);
        }

        // selects a random index for a path
        public int SelectRandomPath(List<(Direction direction, MazeLoc location)> paths)
        {
            return rand.Next(paths.Count);
        }

        // finds paths to take
        public List<(Direction, MazeLoc)> PathFinder(MazeLoc loc)
        {

            List<(Direction, MazeLoc)> pathsToTake = new List<(Direction, MazeLoc)>();

            // directions to check
            int[] p = { loc.x, loc.x + 1, loc.x - 1, loc.y, loc.y, loc.y, loc.x, loc.x, loc.y + 1, loc.y - 1 };

            // direction descrip
            Direction[] directions = new Direction[2];

            // check for available directions
            for(int i = 0; i <=5; i+=5)
            {
                // x or y operation
                directions[0] = (i == 0) ? Direction.FORWARD : Direction.RIGHT;
                directions[1] = (i == 0) ? Direction.BACKWARD : Direction.LEFT; 

                // is there a path?
                var paths = PathFindHelper(p[i], p[i + 1], p[i + 2], p[i + 3], p[i+4], directions);

                // is paths null ? 
                if(paths != null && paths.Count > 0)
                {
                    // add paths to available paths
                    foreach(var path in paths)
                    {
                        pathsToTake.Add(path);
                    }
                }
            }

            // return paths
            return pathsToTake;
        }



        // choose available paths that have not been choosen
        public List<(Direction, MazeLoc)> PathFindHelper(int isAxis, int x_1, int x_2, int y_1, int y_2, Direction[] dir)
        {
            // contains available paths that have not been choosen
            List<(Direction,MazeLoc)> availablePaths = new List<(Direction,MazeLoc)>();

            // checking available paths

            // checking down and right
            if (isAxis < maze.board.GetUpperBound(0) && 
                !maze_navigate.Any(entry=>entry.Item2.Equals(new MazeLoc(x_1, y_1))))
            {
                // is path 
                bool isTrue = (maze.board[x_1, y_1] == '.');

                if (isTrue) availablePaths.Add((dir[0], new MazeLoc(x_1,y_1)));
            }

            // checking up and left
            if(isAxis > maze.board.GetLowerBound(0) && 
                !maze_navigate.Any(entry=>entry.Item2.Equals(new MazeLoc(x_2, y_2))))
            {
                // ispath
                bool isTrue = (maze.board[x_2, y_2] == '.');

                if (isTrue) availablePaths.Add((dir[1], new MazeLoc(x_2,y_2)));
            }

            return availablePaths;
        }

    }
}
