import { Form, Button } from "react-bootstrap";
import { useNavigate } from "react-router-dom";

export function TodoForm({ form, handler, disable, showButtons, backRoute }) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = form;

  const navigate = useNavigate();

  return (
    <>
      <form onSubmit={handleSubmit((data) => handler(data))}>
        <div className="d-flex gap-3 mb-3">
          {/** TASK NAME */}
          <Form.Group className="flex-grow-1">
            <Form.Label>Task Name</Form.Label>
            <Form.Control
              type="text"
              id="task"
              disabled={disable}
              {...register("todo.task", {
                required: "Task is required!",
              })}
            />
            {errors?.todo?.task && (
              <small className="text-danger">
                {errors?.todo?.task?.message}
              </small>
            )}
          </Form.Group>
          {/* DUE DATE */}
          <Form.Group className="flex-grow-1">
            <Form.Label>Due Date</Form.Label>
            <Form.Control
              type="date"
              id="date"
              disabled={disable}
              {...register("todo.dueDate", {
                required: "Due Date is required!",
              })}
            />
            {errors?.todo?.dueDate && (
              <small className="text-danger">
                {errors?.todo?.dueDate?.message}
              </small>
            )}
          </Form.Group>
        </div>
        <div className="d-flex gap-3 mb-3">
          {/* STATUS */}
          <Form.Group className="flex-grow-1">
            <Form.Label>Status</Form.Label>
            <Form.Select
              id="status"
              disabled={disable}
              {...register("todo.status", {
                required: "Status is required!",
              })}
            >
              <option value="">Select..</option>
              <option value="Completed">Completed</option>
              <option value="Pending">Pending</option>
            </Form.Select>
            {errors?.todo?.status && (
              <small className="text-danger">
                {errors?.todo?.status?.message}
              </small>
            )}
          </Form.Group>
          {/* PRIORITY */}
          <Form.Group className="flex-grow-1">
            <Form.Label>Priority</Form.Label>
            <Form.Select
              id="priority"
              disabled={disable}
              {...register("todo.priority", {
                required: "Priority is required!",
              })}
            >
              <option value="">Select..</option>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </Form.Select>
            {errors?.todo?.priority && (
              <small className="text-danger">
                {errors?.todo?.priority?.message}
              </small>
            )}
          </Form.Group>
        </div>

        {/** Description */}
        <Form.Group>
          <Form.Label>Description</Form.Label>
          <Form.Control
            id="description"
            as="textarea"
            rows={5}
            disabled={disable}
            {...register("todo.description")}
          />
        </Form.Group>

        {showButtons && (
          <div className="d-flex justify-content-center align-items-center gap-2 mt-2">
            <Button variant="primary" type="submit">
              Save
            </Button>
            <Button
              variant="secondary"
              type="button"
              onClick={() => {
                navigate(`/${backRoute}`);
              }}
            >
              Cancel
            </Button>
          </div>
        )}
      </form>
    </>
  );
}
