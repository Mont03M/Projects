# C# Projects

## Rat-N-Maze Program

This program is written in C# and solves the classic Rat-in-a-Maze problem using recursion to efficiently navigate an 8×8 maze. The maze is currently hardcoded, but future versions will include functionality to generate randomized mazes. This will allow each execution to produce a unique maze, better demonstrating the algorithm’s ability to find a path to the destination under varying conditions.

To run the program, simply download the project files and execute them using an IDE such as Microsoft Visual Studio or any other environment that supports C#.

## Knights Tour Solver Program

The following program is written in C# and attempts to solve the Knight’s Tour problem for both closed and open tours. It uses recursion, backtracking, and Warnsdorff’s Rule (the knight should always move to an unvisited adjacent square with the fewest possible onward moves). A solution is not always guaranteed, depending on the size of the board.

In addition to Warnsdorff’s Rule, a combination of tie-breakers is utilized to further prioritize moves based on additional criteria such as edge proximity, corner proximity, and Euclidean distance.

The board size can range from 5×5 (25 squares) to 20×20 (400 squares).

## ChessGame

A C# WPF chess application built using the **MVVM architecture**, with a focus on clean separation of game state, move generation, legality validation, UI presentation, asynchronous game processing, and AI strategy integration. This repository is primarily a full-featured chess application that contains a complete rules engine. It implements per-piece pseudo-legal move generation, legality filtering, special moves (castling, en passant, promotion), and game-state detection, so it can fully enforce chess rules and produce legal moves. In short this codebase is a playable chess application with an internal rules/move generator and full game-state handling, suitable for UI play and engine integration.

# MERN Projects

## Todo App

The Todo App is built using the MERN stack (MongoDB, Express.js, React.js, Node.js), providing a scalable architecture that can grow with additional features, modules, routes, and data.

The frontend is developed with React.js and offers a robust interface for viewing, editing, creating, and deleting todos. It includes filtering options to sort todos efficiently and a search bar to find todos based on keywords.

The backend follows the MRC (Model, Route, Controller) pattern to facilitate scalability and optimize query performance. It is built using Node.js and Express.js, and is supported by a MongoDB database.

# React + ASP.NET Core

## Todo APP

The Todo App is built using React, ASP.NET Core Web API, and a MongoDB database, providing a scalable and robust foundation that can easily grow with additional features, modules, routes, and data.

The frontend is developed with React.js, delivering a responsive user experience and supporting the same core functionality as the Todo app found in the MERN projects folder.

The backend is implemented using a clean, maintainable layered architecture, organized into Controllers, Services, DTOs, and Models. This structure promotes separation of concerns, making the codebase easier to extend, test, and maintain as the application evolves.

This design allows for future enhancements such as authentication, advanced filtering, and additional business logic without major restructuring.

# Python Projects

## Logistic Model - Predicting Fractures Based on Bone Mineral Density (BMD)

This program is written in Python and utilizes a GUI interface to accept user inputs and estimate the likelihood of fracture risk in patients. Predictions are based on age, sex, weight (kg), height (cm), and bone mineral density (BMD).

Once the inputs are entered, the data is fed into a logistic regression model to generate predictions. The model evaluates fracture risk across a specified age range (age 1 < age 2) and is highly dependent on BMD values.
