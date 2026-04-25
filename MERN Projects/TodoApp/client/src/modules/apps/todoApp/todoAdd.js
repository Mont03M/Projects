import { CustomAppContainer } from "../../components/customAppContainer";
import { CustomContentHeader } from "../../components/customContentHeader";
import { CustomContentBody } from "../../components/customContentBody";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { TodoForm } from "../../components/todoForm";
import AppRoutes from "./_config";
import { API } from "../../utils/apiService";
import { Container } from "react-bootstrap";

export function TodoAdd() {
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

    API.post("/", data.todo).then((response) => {
      navigate(`/${AppRoutes.baseRoute}`);
    });
  };

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
