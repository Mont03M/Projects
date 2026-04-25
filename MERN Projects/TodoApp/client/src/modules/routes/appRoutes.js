import React from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import { TodoAppRoutes } from "../apps/todoApp";
import { NotFound } from "../components/notFound";
import { Navigate } from "react-router-dom";

export function AppRoutes() {
  return (
    <Routes>
      {/** default redirect -- start page */}
      <Route path="/" element={<Navigate to="/todo-app" replace />} />
      {/** Todo App */}
      <Route path="/todo-app/*" element={<TodoAppRoutes />} />
      {/** Not Found */}
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}
