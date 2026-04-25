using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnightsTour.Heuristics
{
    public class HeuristicWeights
    {
        public double DegreeWeight;
        public double EdgeWeight;
        public double CornerWeight;
        public double CenterWeight;
        public double StartWeight;
        public double RandomWeight;

        public HeuristicWeights(double degree, double edge, double corner, double center, double start, double rand)
        {
            DegreeWeight = degree;
            EdgeWeight = edge;
            CornerWeight = corner;
            CenterWeight = center;
            StartWeight = start;
            RandomWeight = rand;
        }
    }
}
