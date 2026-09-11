# Rat-N-Maze Program

This program is written in C# and solves the classic Rat-in-a-Maze problem using recursion to efficiently navigate an 8×8 maze. The maze is currently hardcoded, but future versions will include functionality to generate randomized mazes. This will allow each execution to produce a unique maze, better demonstrating the algorithm’s ability to find a path to the destination under varying conditions.

To run the program, simply download the project files and execute them using an IDE such as Microsoft Visual Studio or any other environment that supports C#.

## Docker Setup

### Docker file

The project includes a dockerfile to simplify setup.

#### Commands

Build --> docker build -t {tag name} . (e.g., docker build -t rat-n-maze:latest)<br>
Run --> docker run -it {image name} . (e.g., docker run -it rat-n-maze)<br>

The Docker container for the Rat-N-Maze program must be run in an interactive terminal using the -it flag<br>
