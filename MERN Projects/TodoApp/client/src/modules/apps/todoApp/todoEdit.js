import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { useLocation, useParams, useNavigate } from "react-router-dom";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { FormatDate } from "../../helpers/formatDate";
import { TodoForm } from "../../components/todoForm";
import AppRoutes from "./_config";
import { API } from "../../utils/apiService";
import { Spinner, Container } from "react-bootstrap";

export function TodoEdit() {
  const { id } = useParams();
  const { state } = useLocation();
  const navigate = useNavigate();

  const form = useForm({
    defaultValues: {
      todo: {
        _id: "",
        task: "",
        dueDate: "",
        status: "",
        priority: "",
        description: "",
      },
    },
  });

  const { watch, reset } = form;
  const todo = watch("todo");

  const handleEditTodo = (data) => {
    if (!data || !data.todo._id) return;

    API.patch(`/${data.todo._id}`, data.todo).then((response) => {
      navigate(`/${AppRoutes.baseRoute}`);
    });
  };

  useEffect(() => {
    const loadTodo = async () => {
      if (state) {
        reset({
          todo: {
            ...state,
            dueDate: FormatDate(state.dueDate),
          },
        });
        return;
      }

      const response = await API.get(`/${id}`).catch((err) => {
        console.error(err.message);
      });

      if (response?.data?.success) {
        const todo = response.data.data;

        reset({
          todo: {
            ...todo,
            dueDate: FormatDate(todo.dueDate),
          },
        });
      }
    };

    loadTodo();
  }, [id, state, reset]);

  if (todo._id === "") {
    return (
      <div className="d-flex justify-content-center align-items-center vh-100">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  return (
    <>
      <CustomAppContainer>
        <CustomContentHeader>
          <h1 className="text-center w-100">Todo App Edit</h1>
        </CustomContentHeader>
        <CustomContentBody>
          {/** Todo Edit form */}
          <Container style={{ maxWidth: "700px" }}>
            <TodoForm
              form={form}
              handler={handleEditTodo}
              disable={false}
              showButtons={true}
              backRoute={AppRoutes.baseRoute}
            />
          </Container>
        </CustomContentBody>
      </CustomAppContainer>
    </>
  );
}
