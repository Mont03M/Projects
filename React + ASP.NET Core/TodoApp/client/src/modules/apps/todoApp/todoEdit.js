import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { useLocation, useParams, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { FormatDate } from "../../helpers/formatDate";
import { TodoForm } from "../../components/todoForm";
import { AppRoutes } from "./_config";
import { API } from "../../services/apiService";
import { Spinner, Container } from "react-bootstrap";

export function TodoEdit() {
  const { id } = useParams();
  const { state } = useLocation();
  const navigate = useNavigate();
  const [status, setStatus] = useState("idle");
  const [message, setMesage] = useState("Failed to load todo!");

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

  const handleEditTodo = (data) => {
    if (!data || !data.todo._id) return;

    setStatus("loading");

    API.patch(`/${AppRoutes.relativeRoute}/${data.todo._id}`, data.todo)
      .then((response) => {
        console.log(response);
        if (response?.data?.success) {
          setStatus("success");
        } else {
          setStatus("error");
          setMesage("Failed to save any new changes!");
        }

        handleSetTimeout();
      })
      .catch((error) => {
        setStatus("error");
        setMesage("Failed to save any new changes!");
        handleSetTimeout();
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

      const response = await API.get(`/${AppRoutes.relativeRoute}/${id}`).catch(
        (err) => {
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

  if (status === "loading") {
    return (
      <div className="d-flex justify-content-center align-items-center vh-100">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Loading...</span>
        </Spinner>
      </div>
    );
  }

  if (status === "success") {
    return (
      <div className="d-flex flex-column justify-content-center align-items-center vh-100">
        <i
          className="bi bi-check-circle-fill text-success"
          style={{ fontSize: "4rem" }}
        />
        <h4 className="mt-3">Todo Successfully Edited!</h4>
      </div>
    );
  }

  if (status === "error") {
    return (
      <div className="d-flex flex-column justify-content-center align-items-center vh-100">
        <i
          className="bi bi-x-circle-fill text-danger"
          style={{ fontSize: "4rem" }}
        />
        <h4 className="mt-3 text-danger">{message}</h4>
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
