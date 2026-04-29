import { Form, Button } from "react-bootstrap";

export function CustomFilterForm({
  form,
  filterHandler,
  removeHandler,
  showForm,
  setShow,
}) {
  const { register, handleSubmit } = form;

  return (
    <>
      <form onSubmit={handleSubmit((data) => filterHandler(data.filter))}>
        {/* STATUS */}
        <div className="mb-3">
          <label className="form-label fw-bold">Status</label>
          <Form.Select {...register("filter.status")}>
            <option value="">Select..</option>
            <option value="Completed">Completed</option>
            <option value="Pending">Pending</option>
          </Form.Select>
        </div>
        {/* PRIORITY */}
        <div>
          <label className="form-label fw-bold">Priority</label>
          <Form.Select {...register("filter.priority")}>
            <option value="">Select..</option>
            <option value="High">High</option>
            <option value="Medium">Medium</option>
            <option value="Low">Low</option>
          </Form.Select>
        </div>
        {/* DUE DATE */}
        <div className="mb-3">
          <label className="form-label fw-bold">Due Date</label>
          <Form.Control type="date" {...register("filter.dueDate")} />
        </div>
        {/** Buttons */}
        <div className="d-flex justify-content-center align-items-center gap-2 mt-2">
          <Button
            variant="primary"
            type="submit"
            onClick={() => setShow(false)}
          >
            Filter
          </Button>
          <Button
            variant="secondary"
            type="button"
            onClick={() => {
              removeHandler();
              setShow(false);
            }}
          >
            Cancel
          </Button>
        </div>
      </form>
    </>
  );
}
