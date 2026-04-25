import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { useLocation, useParams, useNavigate } from "react-router-dom";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { FaArrowLeft } from "react-icons/fa";
import { FormatDate } from "../../helpers/formatDate";
import { TodoForm } from "../../components/todoForm";
import { API } from "../../utils/apiService";
import { Spinner, Container } from "react-bootstrap";

export function TodoView() {
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
          <div className="position-relative w-100 text-center">
            {/* LEFT BUTTON */}
            <button
              className="btn btn-outline-secondary position-absolute start-0 top-50 translate-middle-y d-flex align-items-center gap-1"
              onClick={() => navigate(-1)}
            >
              <FaArrowLeft />
              Back
            </button>

            {/* CENTER TITLE */}
            <h1 className="mb-0">Todo App View</h1>
          </div>
        </CustomContentHeader>
        <CustomContentBody>
          {/** View Todo form */}
          <Container style={{ maxWidth: "700px" }}>
            <TodoForm form={form} disable={true} showButtons={false} />
          </Container>
        </CustomContentBody>
      </CustomAppContainer>
    </>
  );
}
