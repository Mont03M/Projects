# Projects

## Rat-N-Maze Program

The following program was created in C# and solves the rat-n-maze problem using recursion to improve its search to navigate an 8x8 maze. The maze is hard coded, future versions of the project will contain methods to randomized the construction of a maze. So, that on each iteration the maze will be generated differently, highlighting the capabilities of the algorithms in finding the end of the maze.

All that is needed to run the program is to download the files and execute the program in a Microsoft Visual Studio IDE or similar IDEs that can execute C# code.   

## Knights Tour Solver Program

The following program was created in C# and attempts to solve the knights tour problem for both closed and open tours. Using recursion, backtracking, and Warnsdorffs Rule (the knight should always move to an unvisited adjacent square that has the fewest possible onward moves) a solution is not always found, depending on the size of the board. In addition to Warnsdorffs Rule a combination of tie-breakers are uitilized to sort moves based on other objectives. Such as, edge, corner, and euclidean distance. The size of board can range from 5x5 (25 squares) to 20x20 (400 squares).

Board Sizes: 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20

After sorting moves with the fewest onward moves and a tie with onward moves occurs.

Tie-Breaker 1: Sort by edge distance - select and place moves closest to the edge of the board in the front of the list. A tie occurs then sort by tie-breaker 2<br>
Tie-Breaker 2: Sort by corner distance - select and place moves closet to the corner of the board in the front of the list. A tie occurs then sort by tie-breaker 3<br>
Tie-Breaker 3: Sort by distance (euclidean distance) from the center of the board - select and place the move closest to the center of the board in the front of the list<br> 

### Test Results of KnightTour Algorithm: 

Percentage of Open Tours solved: 80.31%<br>
Even Sizes: 99%<br>
Odd Sizes : 50.58%<br> 

Percentage of Closed Tours Solved: 0.98%<br>
Even Sizes: 0.98%<br>
Odd Sizes: 0%<br>

## Logistic Model - Predicting Fractures Based on Bone Mineral Density (BMD)

The program is created in Python and uitilzies a gui interface to accept inputs from a user to determine the likeiy hood of facture risks on patients based on age, sex, weight_kg, height_cm, and bone mineral density (BMD). Once inputs are entered the data is feed to a logistic model to generate predictions. Where age 1 < age 2, M or F, weight_kg = 0.00, height = 0.00, BMD = 0.00. The results show facture risks on patients from age 1 to age 2 and is higly dependent on BMD inputs.  

The logistic regression model in the program is utilized solve a classification problem. As logistic regression models are excellent at solving binary classification tasks. Essentially, the classification problem was based on whether a patient was likely to get a fracture based on several predictors involving age, sex, weight, height, and bmd, and using the values of fractures as target values. In addition, several metrics were utilized to evaluate the performance of the logistic model and ensure its effectiveness and accuracy when performing predictions.

### Data of the Logistic Model

File: Fractures.xls
Columns: ID, age, sex, fracture, weight_kg, height_cm, medication, waiting_time, bmd.
Data Entries: 170 
Columns Used for Logsitic Model Training: age, sex, fracture, weight_kg, height_cm

### Steps to Running Logistic Model GUI Model:

1. Create a python env (optional) - python -m env [env name] <br>
2. From a an activated env - pip install -r requirements.txt or from a Kernel inside a python IDE - !pip install -r requirements.txt or %pip install -r requirements.txt<br>
3. Make sure the requirements.txt is installing to the correct environment used by the Kernel <br>
4. Make sure the requirements.txt is in the same directory as the model.py and model_results.py files <br>

