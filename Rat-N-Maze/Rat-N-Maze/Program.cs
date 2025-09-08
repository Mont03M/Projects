using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rat_N_Maze
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // objects
            Maze maze = new Maze(8);
            Rat rat = new Rat(maze);

            // maze solver
            SolveMaze path = new SolveMaze(rat);

            // solve maze
            path.Start();
            path.NavigateMaze();
        }
    }

   
}
