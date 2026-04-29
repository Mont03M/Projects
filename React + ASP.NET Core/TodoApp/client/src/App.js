import "./App.css";
import { AppRoutes } from "./modules/routes/appRoutes";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import { TodoProvider } from "./modules/contexts/todoContext";

function App() {
  return (
    <>
      <TodoProvider>
        <AppRoutes />
      </TodoProvider>
    </>
  );
}

export default App;
