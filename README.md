<<<<<<< HEAD
Rat-N-Maze Program

The following program was created in C# and solves the rat-n-maze problem using recursion to improve its search to navigate an 8x8 maze. The maze is hard coded, future versions of the project will contain methods to randomized the construction of a maze. So, that on each iteration the maze will be generated differently, highlighting the capabilities of the algorithms towards finding the end of the maze.

All that is needed to run the program is to download the files and execute the program in a Microsoft Visual Studio IDE or similar IDEs that can execute C# code.   

# Projects
=======
# Projects

## Rat-N-Maze Program

The following program was created in C# and solves the rat-n-maze problem using recursion to improve its search to navigate an 8x8 maze. The maze is hard coded, future versions of the project will contain methods to randomized the construction of a maze. So, that on each iteration the maze will be generated differently, highlighting the capabilities of the algorithms in finding the end of the maze.

All that is needed to run the program is to download the files and execute the program in a Microsoft Visual Studio IDE or similar IDEs that can execute C# code.   

## Knights Tour Solver Program

The following program was created in C# and attempts to solve the knights tour problem for both closed and open tours. Using recursion, backtracking, and Warnsdorffs Rule (the knight should always move to an unvisited adjacent square that has the fewest possible onward moves) a solution is not always found, depending on the size of the board. In addition to Warnsdorffs Rule a combination of tie-breakers are uitilized to sort moves based on other objectives. Such as, edge, corner, and euclidean distance. The size of board can range from 5*5 to 20*20.

Board Sizes: 5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20

After sorting moves with the fewest onward moves and a tie with onward moves occurs.

Tie-Breaker 1: Sort by edge distance - select and place moves closest to the edge of the board in the front of the list. A tie occurs then sort by tie-breaker 2
Tie-Breaker 2: Sort by corner distance - select and place moves closet to the corner of the board in the front of the list. A tie occurs then sort by tie-breaker 3
Tie-Breaker 3: Sort by distance (euclidean distance) from the center of the board - select and place the move closest to the center of the board in the front of the list. 

### Test Results of KnightTour Algorithm: 

Percentage of Open Tours solved: 8.31%
Even Sizes: 99%
Odd Sizes : 50.58% 

Percentage of Closed Tours Solved: 0.98%
Even Sizes: 0.98%
Odd Sizes: 0%

>>>>>>> 8f9ee04 (Second Commit)
