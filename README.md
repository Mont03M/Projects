# C# Projects

## Rat-N-Maze Program

This program is written in C# and solves the classic Rat-in-a-Maze problem using recursion to efficiently navigate an 8×8 maze. The maze is currently hardcoded, but future versions will include functionality to generate randomized mazes. This will allow each execution to produce a unique maze, better demonstrating the algorithm’s ability to find a path to the destination under varying conditions.

To run the program, simply download the project files and execute them using an IDE such as Microsoft Visual Studio or any other environment that supports C#.

### Docker Setup

#### Docker file

The project includes a dockerfile to simplify setup.

##### Commands

Build --> docker build -t {tag name} . (e.g., docker build -t knights-tour:lastest)<br>
Run --> docker run -it {image name} . (e.g., docker run -it knights-tour)<br>

The Docker container for the Knight’s Tour program must be run in an interactive terminal using the -it flag<br>

## Knights Tour Solver Program

The following program is written in C# and attempts to solve the Knight’s Tour problem for both closed and open tours. It uses recursion, backtracking, and Warnsdorff’s Rule (the knight should always move to an unvisited adjacent square with the fewest possible onward moves). A solution is not always guaranteed, depending on the size of the board.

In addition to Warnsdorff’s Rule, a combination of tie-breakers is utilized to further prioritize moves based on additional criteria such as edge proximity, corner proximity, and Euclidean distance.

The board size can range from 5×5 (25 squares) to 20×20 (400 squares).

### Board Sizes

5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20<br>

### Move Selection Strategy

After sorting moves by the fewest onward moves, ties may occur.

#### Tie-breaker 1: Edge Distance

Sort moves by their distance to the edge of the board. Moves closest to the edge are placed at the front of the list.
If a tie still exists, proceed to Tie-Breaker 2.

#### Tie-breaker 2: Corner Distance

Sort moves by their distance to the corners of the board. Moves closest to a corner are placed at the front of the list.
If a tie still exists, proceed to Tie-Breaker 3.

#### Tie-breaker 3: Distance from Center (Euclidean Distance)

Sort moves by their Euclidean distance from the center of the board. Moves closest to the center are placed at the front of the list.

### Test Results of KnightTour Algorithm:

Percentage of Open Tours solved: 80.31%<br>
Even Sizes: 99%<br>
Odd Sizes : 50.58%<br>

Percentage of Closed Tours Solved: 0.98%<br>
Even Sizes: 0.98%<br>
Odd Sizes: 0%<br>

### Docker Setup

#### Docker file

The project includes a dockerfile to simplify setup.

##### Commands

Build --> docker build -t {tag name} . (e.g., docker build -t rat-n-maze:lastest)<br>
Run --> docker run -it {image name} . (e.g., docker run -it rat-n-maze)<br>

The Docker container for the Rat-N-Maze program must be run in an interactive terminal using the -it flag<br>

# MERN Projects

## Todo App

The Todo App is built using the MERN stack (MongoDB, Express.js, React.js, Node.js), providing a scalable architecture that can grow with additional features, modules, routes, and data.

The frontend is developed with React.js and offers a robust interface for viewing, editing, creating, and deleting todos. It includes filtering options to sort todos efficiently and a search bar to find todos based on keywords.

The backend follows the MRC (Model, Route, Controller) pattern to facilitate scalability and optimize query performance. It is built using Node.js and Express.js, and is supported by a MongoDB database.

### Features

#### Filter

##### Parametters:

Status: Completed, Pending<br>
Priority: High, Medium, Low<br>
Due Date: Date object<br>

##### Description:

The filter provides a simple and efficient way to retrieve todos based on specific parameters. It supports using one to three parameters simultaneously and applies them as an AND filter.

#### Filter Badges:

##### Description:

The filter provides a simple and efficient way to retrieve todos based on specific parameters. It supports using one to three parameters simultaneously and applies them as an AND filter.

#### Search:

##### Keywords -> Values:

Name: text<br>
Description: text<br>
Status: Completed, Pending<br>
Priority: High, Medium, Low<br>
Due Date: YYYY-MM-DD<br>

##### Description:

The search bar dynamically updates the UI based on entered keywords, displaying only matching todos. When filters are active, search results are limited to the filtered dataset.

#### Create:

Allows users to create a new todo.

##### Fields include:

Task Name<br>
Due Date<br>
Status<br>
Priority<br>
Description<br>

#### Edit:

Allows users to modify an existing todo.

##### Fields include:

Task Name<br>
Due Date<br>
Status<br>
Priority<br>
Description<br>

#### View:

Allows users to view an existing todo in read-only mode, with all fields disabled to prevent editing.

#### Valdiation:

Both Create and Edit functionalities include form validation to ensure all required fields are properly filled before submission.

#### Docker Setup

##### Docker Compose

The project includes a docker-compose.yml file to integrate the frontend and backend services and simplify setup.

##### Commands

Build & Run --> docker compose up --build<br>
Run --> docker compose up<br>

# Python Projects

## Logistic Model - Predicting Fractures Based on Bone Mineral Density (BMD)

This program is written in Python and utilizes a GUI interface to accept user inputs and estimate the likelihood of fracture risk in patients. Predictions are based on age, sex, weight (kg), height (cm), and bone mineral density (BMD).

Once the inputs are entered, the data is fed into a logistic regression model to generate predictions. The model evaluates fracture risk across a specified age range (age 1 < age 2) and is highly dependent on BMD values.

### Model Overview

The logistic regression model is used to solve a binary classification problem. Logistic regression is well-suited for classification tasks where the outcome is categorical.

In this case, the model predicts whether a patient is likely to experience a fracture based on several predictors, including age, sex, weight, height, and BMD. The target variable is the presence or absence of a fracture.

Additionally, multiple evaluation metrics are used to assess the model’s performance and ensure accuracy and reliability in predictions.

### Dataset Information

File: Fractures.xls<br>
Columns: ID, age, sex, fracture, weight_kg, height_cm, medication, waiting_time, bmd<br>
Total Entries: 170<br>

### Steps to Running Logistic Model GUI:

1. Create a python virtual environment (optional): python -m venv <env_name> <br>
2. Activate the environment: pip install -r requirements.txt<br>
   Alternatively, in a Python IDE or notebook: !pip install -r requirements.txt<br>
   Or: %pip install -r requirements.txt<br>
3. Ensure requirements.txt is installed in the correct environment used by your IDE or kernel. <br>
4. Make sure requirements.txt, model.py, and model_results.py are in the same directory. <br>
