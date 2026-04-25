import { createContext, useContext, useState } from "react";
import { API } from "../utils/apiService";

const TodoContext = createContext();

export function TodoProvider({ children }) {
  const [todos, setTodos] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const getTodos = async () => {
    try {
      setLoading(true);
      setError(null);

      const res = await API.get("/list");

      setTodos(res.data.data);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <TodoContext.Provider value={{ todos, loading, error, getTodos, setTodos }}>
      {children}
    </TodoContext.Provider>
  );
}

export const useTodos = () => useContext(TodoContext);
