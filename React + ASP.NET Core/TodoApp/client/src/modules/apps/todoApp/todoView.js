import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { useLocation, useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { FaArrowLeft } from "react-icons/fa";
import { FormatDate } from "../../helpers/formatDate";
import { TodoForm } from "../../components/todoForm";
import { API } from "../../services/apiService";
import { Spinner, Container } from "react-bootstrap";
import { AppRoutes } from "./_config";

export function TodoView() {
  const { id } = useParams();
  const { state } = useLocation();
  const navigate = useNavigate();
  const [status, setStatus] = useState("idle");

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

  const handleSetTimeout = () => {
    setTimeout(() => {
      navigate(`/${AppRoutes.baseRoute}`);
    }, 2500);
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

      const response = await API.get(`/${AppRoutes.relativeRoute}/${id}`).catch(
        (err) => {
          console.error(err.message);
          setStatus("error");
          handleSetTimeout();
        },
      );

      if (response?.data?.success) {
        const todo = response.data.data;

        reset({
          todo: {
            ...todo,
            dueDate: FormatDate(todo.dueDate),
          },
        });
      } else {
        setStatus("error");
        handleSetTimeout();
      }
    };

    loadTodo();
  }, [id, state, reset]);

  if (status === "error") {
    return (
      <div className="d-flex flex-column justify-content-center align-items-center vh-100">
        <i
          className="bi bi-x-circle-fill text-danger"
          style={{ fontSize: "4rem" }}
        />
        <h4 className="mt-3 text-danger">Failed to load todo!</h4>
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
