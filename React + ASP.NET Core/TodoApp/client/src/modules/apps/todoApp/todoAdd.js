import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { TodoForm } from "../../components/todoForm";
import { AppRoutes } from "./_config";
import { API } from "../../services/apiService";
import { Container, Spinner } from "react-bootstrap";

export function TodoAdd() {
  const [status, setStatus] = useState("idle");
  const navigate = useNavigate();

  const form = useForm({
    defaultValues: {
      todo: {
        task: "",
        dueDate: "",
        status: "Pending",
        priority: "",
        description: "",
      },
    },
  });

  const handleCreateTodo = (data) => {
    if (!data || !data.todo) return;

    setStatus("loading");

    API.post(`/${AppRoutes.relativeRoute}/`, data.todo)
      .then((response) => {
        if (response?.data?.success) {
          setStatus("success");
        } else {
          setStatus("error");
        }

        // show success for 1.5s then redirect
        setTimeout(() => {
          navigate(`/${AppRoutes.baseRoute}`);
        }, 2500);
      })
      .catch(() => {
        setStatus("error");
      });
  };

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
        <h4 className="mt-3">Todo Created Successfully!</h4>
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
        <h4 className="mt-3 text-danger">Failed to create Todo!</h4>
      </div>
    );
  }

  return (
    <>
      <CustomAppContainer>
        <CustomContentHeader>
          <h1 className="text-center w-100">Todo App Create</h1>
        </CustomContentHeader>
        <CustomContentBody>
          {/** Create Todo Form */}
          <Container style={{ maxWidth: "700px" }}>
            <TodoForm
              form={form}
              handler={handleCreateTodo}
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
