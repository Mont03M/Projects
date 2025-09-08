using FsCheck;
using FsCheck.Fluent;
using KnightsTour.Board;
using KnightsTour.DictonaryExtensions;
using KnightsTour.Heuristics;
using KnightsTour.KnightsTourUtilities;
using KnightsTour.Solver;
using KnightsTour.Structs;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;

namespace TestKnightsTour
{
    [TestClass]
    public sealed class Test1
    {


        private ChessBoard chessBoard;
        private KnightTourSolver solver;
        private Dictionary<int, List<WarnsdorffRule>> visitedMoves;
        private BoardLocation[] boardLocations;

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

        public Test1()
        {
            chessBoard = new ChessBoard(8);
            solver = new KnightTourSolver(chessBoard, false, 0);
            visitedMoves = new Dictionary<int, List<WarnsdorffRule>>();

            boardLocations = new[]
            {
                new BoardLocation(1,1),
                new BoardLocation(2,3),
                new BoardLocation(3,4),
                new BoardLocation(3,5)

            };
        }

        public void AddItems()
        {
            visitedMoves[1] = new List<WarnsdorffRule>();
            visitedMoves[2] = new List<WarnsdorffRule>();
            visitedMoves[3] = new List<WarnsdorffRule>();

            visitedMoves[1].Add(new WarnsdorffRule(boardLocations[0], 0));
            visitedMoves[2].Add(new WarnsdorffRule(boardLocations[1], 0));
            visitedMoves[3].Add(new WarnsdorffRule(boardLocations[2], 0));

            visitedMoves[3].Add(new WarnsdorffRule(boardLocations[3], 0));


        }
        /*

        /// <summary>
        /// Test the contains method finds the correct objects
        /// </summary>
        [TestMethod]
        public void TestContainsMethod()
        {

            AddItems();

            Assert.IsTrue(visitedMoves.Contains_(1, new WarnsdorffRule(boardLocations[0], 0)));
            Assert.IsTrue(visitedMoves.Contains_(2, new WarnsdorffRule(boardLocations[1], 0)));
            Assert.IsTrue(visitedMoves.Contains_(3, new WarnsdorffRule(boardLocations[2], 0)));
            Assert.IsTrue(visitedMoves.Contains_(3, new WarnsdorffRule(boardLocations[3], 0)));
        }

        [TestMethod]
        public void TestEuclideanDistanceMethod()
        {
            int size = 7;

            BoardLocation centerOfBoard = new BoardLocation(size / 2, size / 2);

            var objs = warndorffObject([0,1,2,3,4,0,1], [3,2,4,1,5, 0, 0], size);


            double[] calculatedValues =
            {
                3,
                2.23606797749979,
                1.4142135623730951,
                2,
                2.23606797749979,
                4.242640687119285,
                3.605551275463989
            };

            int counter = 0;

            foreach(var move in objs)
            {
                WarnsdorffHeuristics.CalculateEuclideanDistance(move, size, 0, TestMode: true);
                Debug.WriteLine($"Calculations: {move.ToStringObj()}\n Calculated Values: {calculatedValues[counter]}");
                Assert.IsTrue(move.euclideanDistance == calculatedValues[counter]);
                counter++;
            }
        }

        /// <summary>
        /// Test the euclidean distance swap method - correctly tie-breaks degrees with the same degree
        /// if a degree in the list contains the same degree, swap the highest euclidean distance in the front of the 
        /// least - only for degrees with the same degree. Maintaining an acsending order based on degrees
        /// </summary>
        [TestMethod]
        public void TestWarndorffHeuristicsMethod()
        {
            var gen = GenerateWarnsdorffMove().Generator;
            List<WarnsdorffRule> samples = gen.Sample(10, 1).ToList();

            var visitedMoves = new Dictionary<int, List<WarnsdorffRule>>();


            var obj = new WarnsdorffRule
            {
                boardLocation = samples[0].boardLocation
            };

            visitedMoves[0] = new List<WarnsdorffRule>();
            visitedMoves[0].Add(obj);

            var moves = WarnsdorffHeuristics.WarnsdorffHeuristic(visitedMoves, new List<WarnsdorffRule>(), 
                new BoardLocation(4,5), new BoardLocation(0,0), OffSetValues, new ChessBoard(8), 0);

            for (int i = 1; i < moves.Count; i++)
            {
                if (moves[i - 1].Degree == moves[i].Degree)
                {
                    Assert.IsTrue(moves[i - 1].euclideanDistance >= moves[i].euclideanDistance);

                    if (moves[i - 1].euclideanDistance == moves[i].euclideanDistance)
                        Assert.IsTrue(moves[i - 1].boardLocation >= moves[i].boardLocation);
                }

                Assert.IsTrue(moves[i].Degree <= moves[i].Degree);
            }
        }
        */

        [TestMethod]
        public void TestPercentageOfAlgorithm()
        {
            int testCases = 10;
            int size = 7;
            double score = 0;
            double TotalScore = 0;

            bool IsClosedTour = true;

            List<BoardLocation> chessBoardLocations = ChessBoardLocations(size, new List<BoardLocation>());

            for(int i = 0; i < testCases; i++)
            {
                for (int j = 0; j < size * size; j++)
                {
                    var isSolved = new KnightTourSolver(new ChessBoard(size), IsClosedTour, pauseTime: 0)
                      .TestKnightsTourDriver(chessBoardLocations[j], chessBoardLocations);
                    if (isSolved)
                        score += 1;

                   //Debug.WriteLine($"Is solved --- {isSolved} -- Iteration -- {j + 1} -- {chessBoardLocations[j].ToString()} -- {chessBoardLocations.IndexOf(chessBoardLocations[j])}");

                }

                TotalScore += (score / (size * size));
                score = 0;

                //Debug.WriteLine($"Total Score --- {TotalScore} Test Cases -- {i + 1}");
            }

            TotalScore /= testCases;

            Debug.WriteLine($"Total Score percentage: {TotalScore} -- 25 runs -- 10 Test Cases");
        }



        public static List<BoardLocation> ChessBoardLocations(int size, List<BoardLocation> chessBoardLocations)
        {
            chessBoardLocations = new List<BoardLocation>();

            // create (x,y) coordinates
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    chessBoardLocations.Add(new BoardLocation(x, y));
                }
            }

            // return starting location
            return chessBoardLocations;
        }

        /// <summary>
        /// Generates a random object
        /// </summary>
        /// <returns></returns>
        public static Arbitrary<WarnsdorffRule> GenerateWarnsdorffMove()
        {

            // FsCheck generators
            // Create base generators
            Gen<int> degreeGen = Gen.Choose(1, 8);
            Gen<double> euclideanGen = Gen.Constant(0).Select(_ =>
            {
                Random rnd = new Random();
                return 1 + rnd.NextDouble() * (4 - 1); // [1, 3)
            });
            Gen<int> coordGen = Gen.Choose(0, 7);

            // Create object
            var nextMoveGen =
                from deg in degreeGen
                from ed in euclideanGen
                from x in coordGen
                from y in coordGen
                select new WarnsdorffRule
                {
                    boardLocation = new BoardLocation(x, y),
                    Degree = deg,
                    euclideanDistance = ed,
                };

            // return object
            return nextMoveGen.ToArbitrary();
        }

        public static List<WarnsdorffRule> warndorffObject(int[] x, int[] y, int size)
        {
            List<WarnsdorffRule> XY = new List<WarnsdorffRule>();

            for(int i =0; i < size; i++)
            {
                XY.Add(new WarnsdorffRule
                {
                    boardLocation = new BoardLocation(x[i], y[i])
                });
            }

            return XY;
        }
    }
}
