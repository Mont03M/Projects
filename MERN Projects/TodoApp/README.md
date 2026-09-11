# Todo App

The Todo App is built using the MERN stack (MongoDB, Express.js, React.js, Node.js), providing a scalable architecture that can grow with additional features, modules, routes, and data.

The frontend is developed with React.js and offers a robust interface for viewing, editing, creating, and deleting todos. It includes filtering options to sort todos efficiently and a search bar to find todos based on keywords.

The backend follows the MRC (Model, Route, Controller) pattern to facilitate scalability and optimize query performance. It is built using Node.js and Express.js, and is supported by a MongoDB database.

## Features

### Filter

#### Parametters:

Status: Completed, Pending<br>
Priority: High, Medium, Low<br>
Due Date: Date object<br>

#### Description:

The filter provides a simple and efficient way to retrieve todos based on specific parameters. It supports using one to three parameters simultaneously and applies them as an AND filter.

### Filter Badges:

#### Description:

The filter provides a simple and efficient way to retrieve todos based on specific parameters. It supports using one to three parameters simultaneously and applies them as an AND filter.

### Search:

#### Keywords -> Values:

Name: text<br>
Description: text<br>
Status: Completed, Pending<br>
Priority: High, Medium, Low<br>
Due Date: YYYY-MM-DD<br>

#### Description:

The search bar dynamically updates the UI based on entered keywords, displaying only matching todos. When filters are active, search results are limited to the filtered dataset.

### Create:

Allows users to create a new todo.

#### Fields include:

Task Name<br>
Due Date<br>
Status<br>
Priority<br>
Description<br>

### Edit:

Allows users to modify an existing todo.

#### Fields include:

Task Name<br>
Due Date<br>
Status<br>
Priority<br>
Description<br>

### View:

Allows users to view an existing todo in read-only mode, with all fields disabled to prevent editing.

### Valdiation:

Both Create and Edit functionalities include form validation to ensure all required fields are properly filled before submission.

### Docker Setup

#### Docker Compose

The project includes a docker-compose.yml file to integrate the frontend and backend services and simplify setup.

#### Commands

Build & Run --> docker compose up --build<br>
Run --> docker compose up<br>
