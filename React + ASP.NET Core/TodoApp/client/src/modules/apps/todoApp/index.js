import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import { TodoApp } from "./todoApp";
import { TodoAdd } from "./todoAdd";
import { TodoEdit } from "./todoEdit";
import { TodoView } from "./todoView";
import { NotFound } from "../../components/notFound";

export function TodoAppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<TodoApp />} />
      <Route path="add" element={<TodoAdd />} />
      <Route path="edit/:id" element={<TodoEdit />} />
      <Route path="view/:id" element={<TodoView />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
