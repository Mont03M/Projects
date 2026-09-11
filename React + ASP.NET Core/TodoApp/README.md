# Todo APP

The Todo App is built using React, ASP.NET Core Web API, and a MongoDB database, providing a scalable and robust foundation that can easily grow with additional features, modules, routes, and data.

The frontend is developed with React.js, delivering a responsive user experience and supporting the same core functionality as the Todo app found in the MERN projects folder.

The backend is implemented using a clean, maintainable layered architecture, organized into Controllers, Services, DTOs, and Models. This structure promotes separation of concerns, making the codebase easier to extend, test, and maintain as the application evolves.

This design allows for future enhancements such as authentication, advanced filtering, and additional business logic without major restructuring.

## New Features

### Notification Messages

Notification messages provide immediate visual feedback for user actions such as creating, editing, and deleting todos.

When a todo is successfully created or updated, a green checkmark appears in the center of the UI to confirm the action. If an error occurs during creation, editing, or viewing, a red “X” icon is displayed in the same central position to clearly indicate failure.

With the exception of delete actions, all notifications remain visible for 2.5 seconds before the user is automatically redirected to the main page. This brief delay ensures users have enough time to recognize the outcome of their action without interrupting the overall flow of the application.

These notifications improve usability by making system responses clear, consistent, and easy to understand.

### Docker Setup

#### Docker Compose

The project includes a docker-compose.yml file to integrate the frontend and backend services and simplify setup.

#### Commands

Build & Run --> docker compose up --build<br>
Run --> docker compose up<br>
