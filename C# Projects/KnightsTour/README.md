# Knights Tour Solver Program

The following program is written in C# and attempts to solve the Knight’s Tour problem for both closed and open tours. It uses recursion, backtracking, and Warnsdorff’s Rule (the knight should always move to an unvisited adjacent square with the fewest possible onward moves). A solution is not always guaranteed, depending on the size of the board.

In addition to Warnsdorff’s Rule, a combination of tie-breakers is utilized to further prioritize moves based on additional criteria such as edge proximity, corner proximity, and Euclidean distance.

The board size can range from 5×5 (25 squares) to 20×20 (400 squares).

## Board Sizes

5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20<br>

## Move Selection Strategy

After sorting moves by the fewest onward moves, ties may occur.

### Tie-breaker 1: Edge Distance

Sort moves by their distance to the edge of the board. Moves closest to the edge are placed at the front of the list.
If a tie still exists, proceed to Tie-Breaker 2.

### Tie-breaker 2: Corner Distance

Sort moves by their distance to the corners of the board. Moves closest to a corner are placed at the front of the list.
If a tie still exists, proceed to Tie-Breaker 3.

### Tie-breaker 3: Distance from Center (Euclidean Distance)

Sort moves by their Euclidean distance from the center of the board. Moves closest to the center are placed at the front of the list.

## Test Results of KnightTour Algorithm:

Percentage of Open Tours solved: 80.31%<br>
Even Sizes: 99%<br>
Odd Sizes : 50.58%<br>

Percentage of Closed Tours Solved: 0.98%<br>
Even Sizes: 0.98%<br>
Odd Sizes: 0%<br>

## Docker Setup

### Docker file

The project includes a dockerfile to simplify setup.

#### Commands

Build --> docker build -t {tag name} . (e.g., docker build -t knights-tour:latest)<br>
Run --> docker run -it {image name} . (e.g., docker run -it knights-tour)<br>

The Docker container for the Knight’s Tour program must be run in an interactive terminal using the -it flag<br>
